using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;
using UnityStandardAssets.ImageEffects;

namespace SilksongPerformanceMod;

internal static class RuntimeContext
{
    private static readonly RuntimeOverridesSnapshot OverridesSnapshot = new();

    private static float _lastSoftGcTime;

    private static readonly FieldInfo BloomResolutionField =
        AccessTools.Field(typeof(BloomOptimized), "resolution");

    private static readonly object? BloomLowResolutionValue =
        BloomResolutionField != null ? Enum.ToObject(BloomResolutionField.FieldType, 0) : null;

    private static readonly PropertyInfo? AsyncUploadBufferSizeProperty =
        AccessTools.Property(typeof(QualitySettings), "asyncUploadBufferSize");

    private static readonly PropertyInfo? AsyncUploadTimeSliceProperty =
        AccessTools.Property(typeof(QualitySettings), "asyncUploadTimeSlice");

    private static int _gameplayStateCacheFrame = -1;

    private static bool _cachedShouldApplyDistanceThrottle;

    private static int _distanceCacheFrame = -1;

    private static bool _hasCachedHeroPosition;

    private static bool _hasCachedCameraPosition;

    private static Vector3 _cachedHeroPosition;

    private static Vector3 _cachedCameraPosition;

    private static ProcessPriorityClass? _originalProcessPriority;

    private static bool _processPriorityApplied;

    // Cap de FPS alvo para o modo agressivo de GPU.
    // -1 (ilimitado) maximiza CPU/GPU constantemente, podendo causar throttling
    // térmico e reduzir o FPS estável. 240 entrega headroom amplo sem
    // desperdiçar recursos em frames que o monitor não exibirá.
    private const int TargetFrameRateCap = 240;

    public static ManualLogSource Log { get; private set; } = null!;

    public static void Initialize(ManualLogSource log)
    {
        Log = log;
    }

    public static void ApplyImmediateRuntimeOverrides()
    {
        OverridesSnapshot.CaptureIfNeeded();

        if (ModSettings.EnableGcSmoothing.Value)
        {
            TryEnableAutomaticGc("plugin startup");
            GCManager.DisabledManualCollect = true;
        }
        else
        {
            GCManager.DisabledManualCollect = false;
        }

        ApplyMemoryRuntimeOverrides("plugin startup");
        ApplyProcessPriorityOverride("plugin startup");
        ApplyAggressiveGpuMode("plugin startup");
    }

    public static void ResetRuntimeOverrides()
    {
        RestoreProcessPriority();
        OverridesSnapshot.Restore();
        TryEnableAutomaticGc("plugin teardown");
        PlanarReflectionRegistry.Clear();
        PooledEffectPendingCounter.Reset();
    }

    public static void OnConfigurationChanged()
    {
        if (ModSettings.EnableGcSmoothing.Value)
        {
            TryEnableAutomaticGc("config change");
            GCManager.DisabledManualCollect = true;
        }
        else
        {
            GCManager.DisabledManualCollect = false;
        }

        ApplyMemoryRuntimeOverrides("config change");
        ApplyProcessPriorityOverride("config change");
        ApplyAggressiveGpuMode("config change");
    }

    public static bool IsAggressiveGpuModeEnabled()
    {
        return ModSettings.EnableAggressiveGpuMode.Value;
    }

    public static void ApplyAggressiveGpuMode(string source)
    {
        if (!IsAggressiveGpuModeEnabled())
        {
            return;
        }

        try
        {
            OverridesSnapshot.CaptureIfNeeded();

            if (ModSettings.DisableVsyncAndFrameCap.Value)
            {
                // CORRIGIDO: era vSyncCount = 0 e targetFrameRate = -1 (ilimitado).
                // Sem cap, o Unity renderiza 300-500+ FPS, maximizando CPU/GPU e
                // podendo causar throttling térmico. 240 FPS garante headroom para
                // monitores de alta taxa de atualização sem desperdiçar recursos.
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = TargetFrameRateCap;
            }

            if (ModSettings.ForceLowUnityQualitySettings.Value)
            {
                QualitySettings.antiAliasing = 0;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 0f;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.softParticles = false;
            }

            ForceLowGameVideoSettings();
            TuneGraphicsComponents();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to apply aggressive GPU mode from {source}: {ex.Message}");
        }
    }

    public static void TryEnableAutomaticGc(string source)
    {
        try
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to enable automatic GC from {source}: {ex.Message}");
        }
    }

    public static void PerformSoftGc(string source)
    {
        TryEnableAutomaticGc(source);

        if (GarbageCollector.isIncremental)
        {
            GarbageCollector.CollectIncremental((ulong)ModSettings.IncrementalGcBudgetNanoseconds.Value);
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now - _lastSoftGcTime < ModSettings.NonIncrementalGcCooldownSeconds.Value)
        {
            return;
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false, compacting: false);
        _lastSoftGcTime = now;
    }

    public static int? TryReadAsyncUploadBufferSize()
    {
        if (AsyncUploadBufferSizeProperty?.CanRead != true)
        {
            return null;
        }

        return (int?)AsyncUploadBufferSizeProperty.GetValue(null, null);
    }

    public static int? TryReadAsyncUploadTimeSlice()
    {
        if (AsyncUploadTimeSliceProperty?.CanRead != true)
        {
            return null;
        }

        return (int?)AsyncUploadTimeSliceProperty.GetValue(null, null);
    }

    public static void TryWriteAsyncUploadBufferSize(int value)
    {
        SetStaticIntProperty(AsyncUploadBufferSizeProperty, value);
    }

    public static void TryWriteAsyncUploadTimeSlice(int value)
    {
        SetStaticIntProperty(AsyncUploadTimeSliceProperty, value);
    }

    private static void ApplyMemoryRuntimeOverrides(string source)
    {
        if (!ModSettings.EnableAsyncUploadBufferTuning.Value)
        {
            return;
        }

        try
        {
            OverridesSnapshot.CaptureIfNeeded();
            SetStaticIntProperty(AsyncUploadBufferSizeProperty, ModSettings.AsyncUploadBufferSizeMb.Value);
            SetStaticIntProperty(AsyncUploadTimeSliceProperty, ModSettings.AsyncUploadTimeSliceMs.Value);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to apply memory runtime overrides from {source}: {ex.Message}");
        }
    }

    private static void SetStaticIntProperty(PropertyInfo? property, int value)
    {
        if (property?.CanWrite != true)
        {
            return;
        }

        property.SetValue(null, value, null);
    }

    private static void ApplyProcessPriorityOverride(string source)
    {
        if (!ModSettings.EnableHighProcessPriority.Value)
        {
            RestoreProcessPriority();
            return;
        }

        try
        {
            using Process process = Process.GetCurrentProcess();
            _originalProcessPriority ??= process.PriorityClass;
            if (process.PriorityClass != ProcessPriorityClass.High)
            {
                process.PriorityClass = ProcessPriorityClass.High;
            }

            _processPriorityApplied = true;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to apply process priority from {source}: {ex.Message}");
        }
    }

    private static void RestoreProcessPriority()
    {
        if (!_processPriorityApplied || !_originalProcessPriority.HasValue)
        {
            return;
        }

        try
        {
            using Process process = Process.GetCurrentProcess();
            process.PriorityClass = _originalProcessPriority.Value;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to restore process priority: {ex.Message}");
        }
        finally
        {
            _processPriorityApplied = false;
            _originalProcessPriority = null;
        }
    }

    public static bool IsGameplayCriticalState()
    {
        GameManager instance = GameManager.instance;
        if (instance == null)
        {
            return false;
        }

        return instance.GameState is GameState.PLAYING or GameState.PAUSED;
    }

    public static bool IsPerFrameHarmonyThrottleEnabled()
    {
        return ModSettings.EnablePerFrameHarmonyThrottles.Value;
    }

    public static bool ShouldApplyDistanceThrottle()
    {
        int frame = Time.frameCount;
        if (_gameplayStateCacheFrame == frame)
        {
            return _cachedShouldApplyDistanceThrottle;
        }

        _gameplayStateCacheFrame = frame;
        _cachedShouldApplyDistanceThrottle = Application.isPlaying && IsGameplayCriticalState();
        return _cachedShouldApplyDistanceThrottle;
    }

    public static bool AllowBlockingGc()
    {
        return !IsGameplayCriticalState();
    }

    private static void ForceLowGameVideoSettings()
    {
        if (!ModSettings.ForceLowAdvancedVideoSettings.Value)
        {
            return;
        }

        GameManager? gm = GameManager.instance;
        GameSettings? settings = gm?.gameSettings;
        if (settings == null || gm == null)
        {
            return;
        }

        // CORRIGIDO: targetFrameRate alinhado com o cap do Unity acima.
        settings.vSync = 0;
        settings.targetFrameRate = TargetFrameRateCap;
        settings.particleEffectsLevel = 0;
        settings.shaderQuality = ShaderQualities.Low;
        settings.cameraNoise = false;
        gm.RefreshParticleSystems();
    }

    private static void TuneGraphicsComponents()
    {
        GameCameras cameras = GameCameras.SilentInstance;
        if (cameras != null)
        {
            ApplyCameraPerformanceTuning(cameras.mainCamera);
            ApplyCameraPerformanceTuning(cameras.hudCamera);
        }

        PlanarReflectionRegistry.PruneDestroyed();
        PlanarReflectionRegistry.ApplyTuningToAll();
    }

    private static void ApplyCameraPerformanceTuning(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (ModSettings.EnableCameraLowSpecTuning.Value)
        {
            camera.allowHDR = false;
            camera.allowMSAA = false;
        }

        BloomOptimized bloom = camera.gameObject.GetComponent<BloomOptimized>();
        if (bloom != null && ModSettings.EnableBloomLowSpecTuning.Value)
        {
            bloom.BlurIterations = Math.Min(bloom.BlurIterations, ModSettings.MaxBloomBlurIterations.Value);
            if (BloomResolutionField != null && BloomLowResolutionValue != null)
            {
                BloomResolutionField.SetValue(bloom, BloomLowResolutionValue);
            }
        }
    }

    public static void OptimizePlanarReflection(PlanarRealtimeReflection reflection)
    {
        if (reflection == null)
        {
            return;
        }

        reflection.m_DisablePixelLights = true;
        if (ModSettings.EnablePlanarReflectionResolutionReduction.Value)
        {
            reflection.m_TextureResolution = Math.Min(
                reflection.m_TextureResolution,
                ModSettings.PlanarReflectionTextureResolution.Value);
        }
    }

    public static bool ShouldRunEveryNFrames(int seed, int interval)
    {
        if (interval <= 1)
        {
            return true;
        }

        int phase = Math.Abs(seed) % interval;
        return (Time.frameCount + phase) % interval == 0;
    }

    public static bool ShouldRunForDistance(Transform transform, float nearHeroDistance, float nearCameraDistance, int interval)
    {
        if (interval <= 1)
        {
            return true;
        }

        if (transform == null)
        {
            return true;
        }

        if (!ShouldApplyDistanceThrottle())
        {
            return true;
        }

        RefreshDistanceCache();
        Vector3 position = transform.position;

        if (_hasCachedHeroPosition && (_cachedHeroPosition - position).sqrMagnitude <= nearHeroDistance * nearHeroDistance)
        {
            return true;
        }

        if (_hasCachedCameraPosition && (_cachedCameraPosition - position).sqrMagnitude <= nearCameraDistance * nearCameraDistance)
        {
            return true;
        }

        return ShouldRunEveryNFrames(transform.GetInstanceID(), interval);
    }

    private static void RefreshDistanceCache()
    {
        int frame = Time.frameCount;
        if (_distanceCacheFrame == frame)
        {
            return;
        }

        _distanceCacheFrame = frame;

        HeroController? hero = HeroController.instance;
        Transform? heroTransform = hero != null ? hero.transform : null;
        _hasCachedHeroPosition = heroTransform != null;
        if (_hasCachedHeroPosition)
        {
            _cachedHeroPosition = heroTransform!.position;
        }

        GameCameras? cameras = GameCameras.instance;
        Camera? camera = cameras != null ? cameras.mainCamera : null;
        Transform? cameraTransform = camera != null ? camera.transform : null;
        _hasCachedCameraPosition = cameraTransform != null;
        if (_hasCachedCameraPosition)
        {
            _cachedCameraPosition = cameraTransform!.position;
        }
    }
}