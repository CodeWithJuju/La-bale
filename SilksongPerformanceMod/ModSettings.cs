using System;
using BepInEx.Configuration;

namespace SilksongPerformanceMod;

internal static class ModSettings
{
    public static ConfigEntry<bool> EnableGcSmoothing { get; private set; } = null!;

    public static ConfigEntry<bool> ConvertIdleCleanupToSoftGc { get; private set; } = null!;

    public static ConfigEntry<int> IncrementalGcBudgetNanoseconds { get; private set; } = null!;

    public static ConfigEntry<float> NonIncrementalGcCooldownSeconds { get; private set; } = null!;

    public static ConfigEntry<bool> EnableSceneTransitionMemoryCleanup { get; private set; } = null!;

    public static ConfigEntry<int> SceneMemoryCleanupInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAsyncUploadBufferTuning { get; private set; } = null!;

    public static ConfigEntry<int> AsyncUploadBufferSizeMb { get; private set; } = null!;

    public static ConfigEntry<int> AsyncUploadTimeSliceMs { get; private set; } = null!;

    public static ConfigEntry<bool> EnableWaterPhysicsAllocationFix { get; private set; } = null!;

    public static ConfigEntry<bool> EnableHighProcessPriority { get; private set; } = null!;

    public static ConfigEntry<bool> EnableParticleCullingThrottle { get; private set; } = null!;

    public static ConfigEntry<int> ParticleCullingInterval { get; private set; } = null!;

    public static ConfigEntry<float> ParticleNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> ParticleNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableWaterfallParticleThrottle { get; private set; } = null!;

    public static ConfigEntry<int> WaterfallParticleInterval { get; private set; } = null!;

    public static ConfigEntry<float> WaterfallParticleNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> WaterfallParticleNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableParticleAutoDisableThrottle { get; private set; } = null!;

    public static ConfigEntry<int> ParticleAutoDisableInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAggressiveOffscreenCulling { get; private set; } = null!;

    public static ConfigEntry<bool> EnableOffscreenLoopingParticleSuspend { get; private set; } = null!;

    public static ConfigEntry<bool> EnableOffscreenVisualRendererDisable { get; private set; } = null!;

    public static ConfigEntry<bool> EnableOffscreenDecorativeBehaviourDisable { get; private set; } = null!;

    public static ConfigEntry<int> OffscreenCullingScanIntervalFrames { get; private set; } = null!;

    public static ConfigEntry<int> OffscreenCullingMaxCandidatesPerFrame { get; private set; } = null!;

    public static ConfigEntry<float> OffscreenCullingViewportPadding { get; private set; } = null!;

    public static ConfigEntry<float> OffscreenCullingNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableSceneColorThrottle { get; private set; } = null!;

    public static ConfigEntry<int> SceneColorInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableLosThrottle { get; private set; } = null!;

    public static ConfigEntry<int> AlertRangeInterval { get; private set; } = null!;

    public static ConfigEntry<int> LineOfSightInterval { get; private set; } = null!;

    public static ConfigEntry<float> LosNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> LosNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableWalkerThrottle { get; private set; } = null!;

    public static ConfigEntry<int> WalkerInterval { get; private set; } = null!;

    public static ConfigEntry<float> WalkerNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> WalkerNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableFootstepDebounce { get; private set; } = null!;

    public static ConfigEntry<bool> EnableWeaverWalkThreadFastPath { get; private set; } = null!;

    public static ConfigEntry<bool> EnableTk2dQueueDedup { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAraTrailOffscreenCulling { get; private set; } = null!;

    public static ConfigEntry<float> AraTrailOffscreenDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableNestedFadeMixerThrottle { get; private set; } = null!;

    public static ConfigEntry<int> NestedFadeMixerInterval { get; private set; } = null!;

    public static ConfigEntry<float> NestedFadeMixerNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> NestedFadeMixerNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAmbientFloatThrottle { get; private set; } = null!;

    public static ConfigEntry<int> AmbientFloatInterval { get; private set; } = null!;

    public static ConfigEntry<float> AmbientFloatNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> AmbientFloatNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAmbientSwayThrottle { get; private set; } = null!;

    public static ConfigEntry<int> AmbientSwayInterval { get; private set; } = null!;

    public static ConfigEntry<float> AmbientSwayNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> AmbientSwayNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableDistanceSilhouetteThrottle { get; private set; } = null!;

    public static ConfigEntry<int> DistanceSilhouetteInterval { get; private set; } = null!;

    public static ConfigEntry<float> DistanceSilhouetteNearHeroDistance { get; private set; } = null!;

    public static ConfigEntry<float> DistanceSilhouetteNearCameraDistance { get; private set; } = null!;

    public static ConfigEntry<bool> EnableIdlePooledEffectManagerSkip { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAggressiveGpuMode { get; private set; } = null!;

    public static ConfigEntry<bool> ForceLowAdvancedVideoSettings { get; private set; } = null!;

    public static ConfigEntry<bool> ForceLowUnityQualitySettings { get; private set; } = null!;

    public static ConfigEntry<bool> DisableVsyncAndFrameCap { get; private set; } = null!;

    public static ConfigEntry<bool> EnableBloomLowSpecTuning { get; private set; } = null!;

    public static ConfigEntry<int> MaxBloomBlurIterations { get; private set; } = null!;

    public static ConfigEntry<bool> EnableCameraLowSpecTuning { get; private set; } = null!;

    public static ConfigEntry<bool> EnablePlanarReflectionResolutionReduction { get; private set; } = null!;

    public static ConfigEntry<int> PlanarReflectionTextureResolution { get; private set; } = null!;

    public static ConfigEntry<bool> EnableUberPostprocessModuleCache { get; private set; } = null!;

    public static ConfigEntry<int> UberPostprocessRefreshInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableLagHitSourceIndex { get; private set; } = null!;

    public static ConfigEntry<bool> EnableDamageEnemiesCooldownFastPath { get; private set; } = null!;

    public static ConfigEntry<bool> EnableRealtimeReflectionTuning { get; private set; } = null!;

    public static ConfigEntry<int> MaxRealtimeReflectionCubemapSize { get; private set; } = null!;

    public static ConfigEntry<int> RealtimeReflectionFrameInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableLightBlurTextureReduction { get; private set; } = null!;

    public static ConfigEntry<int> MaxLightBlurRenderTextureHeight { get; private set; } = null!;

    public static ConfigEntry<bool> EnableCameraRenderToMeshThrottle { get; private set; } = null!;

    public static ConfigEntry<int> CameraRenderToMeshInterval { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAudioEventManagerIdleSkip { get; private set; } = null!;

    public static ConfigEntry<bool> EnableLowPassDistanceIdleSkip { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAnimatorGroupIdleSkip { get; private set; } = null!;

    public static ConfigEntry<bool> EnableAggressiveEffectsThrottle { get; private set; } = null!;

    public static ConfigEntry<int> AggressiveEffectsInterval { get; private set; } = null!;

    public static void Bind(ConfigFile config)
    {
        EnableGcSmoothing = config.Bind(
            "GC",
            "EnableGcSmoothing",
            true,
            "Keeps Unity GC enabled and replaces blocking manual collections during gameplay with softer collections.");

        ConvertIdleCleanupToSoftGc = config.Bind(
            "GC",
            "ConvertIdleCleanupToSoftGc",
            true,
            "Prevents IdleMemoryCleaner from forcing a blocking compacting collection.");

        IncrementalGcBudgetNanoseconds = config.Bind(
            "GC",
            "IncrementalGcBudgetNanoseconds",
            5_000_000,
            new ConfigDescription(
                "Incremental GC budget used when the runtime supports incremental collection.",
                new AcceptableValueRange<int>(500_000, 25_000_000)));

        NonIncrementalGcCooldownSeconds = config.Bind(
            "GC",
            "NonIncrementalGcCooldownSeconds",
            10f,
            new ConfigDescription(
                "Minimum time between soft non-incremental collections while gameplay is active.",
                new AcceptableValueRange<float>(1f, 60f)));

        EnableSceneTransitionMemoryCleanup = config.Bind(
            "Memory",
            "EnableSceneTransitionMemoryCleanup",
            true,
            "Forces unused asset cleanup during scene transitions on a configurable interval. This targets RAM/VRAM residue without changing the visible scene.");

        SceneMemoryCleanupInterval = config.Bind(
            "Memory",
            "SceneMemoryCleanupInterval",
            2,
            new ConfigDescription(
                "Number of scene transitions between forced unused asset cleanup runs.",
                new AcceptableValueRange<int>(1, 8)));

        EnableAsyncUploadBufferTuning = config.Bind(
            "Memory",
            "EnableAsyncUploadBufferTuning",
            true,
            "Lowers Unity's async texture upload buffer to reduce reserved memory without changing texture quality.");

        AsyncUploadBufferSizeMb = config.Bind(
            "Memory",
            "AsyncUploadBufferSizeMb",
            8,
            new ConfigDescription(
                "Target Unity async upload buffer size in MB.",
                new AcceptableValueRange<int>(2, 64)));

        AsyncUploadTimeSliceMs = config.Bind(
            "Memory",
            "AsyncUploadTimeSliceMs",
            2,
            new ConfigDescription(
                "Target Unity async upload time slice in milliseconds.",
                new AcceptableValueRange<int>(1, 16)));

        EnableWaterPhysicsAllocationFix = config.Bind(
            "Memory",
            "EnableWaterPhysicsAllocationFix",
            true,
            "Reuses WaterPhysics spread buffers instead of allocating temporary arrays every physics tick.");

        EnableHighProcessPriority = config.Bind(
            "Process",
            "EnableHighProcessPriority",
            false,
            "Raises the game process priority to High while the plugin is loaded. Off by default; avoid Realtime priority.");

        EnableParticleCullingThrottle = config.Bind(
            "Particles",
            "EnableParticleCullingThrottle",
            true,
            "Runs ParticleCulling less often for distant particle roots.");

        ParticleCullingInterval = config.Bind(
            "Particles",
            "ParticleCullingInterval",
            4,
            new ConfigDescription(
                "Frame interval used for distant ParticleCulling roots.",
                new AcceptableValueRange<int>(2, 12)));

        ParticleNearHeroDistance = config.Bind(
            "Particles",
            "ParticleNearHeroDistance",
            18f,
            new ConfigDescription(
                "Particle roots closer than this to the hero are updated every frame.",
                new AcceptableValueRange<float>(4f, 64f)));

        ParticleNearCameraDistance = config.Bind(
            "Particles",
            "ParticleNearCameraDistance",
            24f,
            new ConfigDescription(
                "Particle roots closer than this to the camera are updated every frame.",
                new AcceptableValueRange<float>(4f, 80f)));

        EnableWaterfallParticleThrottle = config.Bind(
            "Particles",
            "EnableWaterfallParticleThrottle",
            true,
            "Runs distant WaterfallParticles raycast checks less often when they are far from both the hero and camera.");

        WaterfallParticleInterval = config.Bind(
            "Particles",
            "WaterfallParticleInterval",
            4,
            new ConfigDescription(
                "Frame interval used for distant WaterfallParticles.Update calls.",
                new AcceptableValueRange<int>(2, 12)));

        WaterfallParticleNearHeroDistance = config.Bind(
            "Particles",
            "WaterfallParticleNearHeroDistance",
            18f,
            new ConfigDescription(
                "WaterfallParticles closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(4f, 80f)));

        WaterfallParticleNearCameraDistance = config.Bind(
            "Particles",
            "WaterfallParticleNearCameraDistance",
            30f,
            new ConfigDescription(
                "WaterfallParticles closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(4f, 120f)));

        EnableParticleAutoDisableThrottle = config.Bind(
            "Particles",
            "EnableParticleAutoDisableThrottle",
            true,
            "Runs ParticleSystemAutoDisable polling less often; this only delays recycling finished particle effects by a few frames.");

        ParticleAutoDisableInterval = config.Bind(
            "Particles",
            "ParticleAutoDisableInterval",
            4,
            new ConfigDescription(
                "Frame interval used for ParticleSystemAutoDisable.Update.",
                new AcceptableValueRange<int>(2, 12)));

        EnableAggressiveOffscreenCulling = config.Bind(
            "OffscreenCulling",
            "EnableAggressiveOffscreenCulling",
            true,
            "Aggressively suspends safe visual-only work when it is outside the camera padding and away from the hero.");

        EnableOffscreenLoopingParticleSuspend = config.Bind(
            "OffscreenCulling",
            "EnableOffscreenLoopingParticleSuspend",
            true,
            "Pauses and clears safe looping ParticleSystems while they are off-screen. One-shot particles are not paused.");

        EnableOffscreenVisualRendererDisable = config.Bind(
            "OffscreenCulling",
            "EnableOffscreenVisualRendererDisable",
            true,
            "Disables safe visual-only renderers while they are off-screen.");

        EnableOffscreenDecorativeBehaviourDisable = config.Bind(
            "OffscreenCulling",
            "EnableOffscreenDecorativeBehaviourDisable",
            true,
            "Disables known decorative animation behaviours while their object is off-screen.");

        OffscreenCullingScanIntervalFrames = config.Bind(
            "OffscreenCulling",
            "OffscreenCullingScanIntervalFrames",
            180,
            new ConfigDescription(
                "Frame interval used to scan for new off-screen culling candidates.",
                new AcceptableValueRange<int>(60, 600)));

        OffscreenCullingMaxCandidatesPerFrame = config.Bind(
            "OffscreenCulling",
            "OffscreenCullingMaxCandidatesPerFrame",
            80,
            new ConfigDescription(
                "Maximum off-screen culling candidates processed per frame.",
                new AcceptableValueRange<int>(20, 1000)));

        OffscreenCullingViewportPadding = config.Bind(
            "OffscreenCulling",
            "OffscreenCullingViewportPadding",
            8f,
            new ConfigDescription(
                "World-space padding around the camera before visual-only objects can be suspended.",
                new AcceptableValueRange<float>(1f, 40f)));

        OffscreenCullingNearHeroDistance = config.Bind(
            "OffscreenCulling",
            "OffscreenCullingNearHeroDistance",
            24f,
            new ConfigDescription(
                "Visual-only objects closer than this to the hero remain active even if outside the camera padding.",
                new AcceptableValueRange<float>(4f, 120f)));

        EnableSceneColorThrottle = config.Bind(
            "PostFX",
            "EnableSceneColorThrottle",
            true,
            "Throttles SceneColorManager.UpdateScript during regular gameplay interpolation.");

        SceneColorInterval = config.Bind(
            "PostFX",
            "SceneColorInterval",
            3,
            new ConfigDescription(
                "Frame interval used for non-forced SceneColorManager updates.",
                new AcceptableValueRange<int>(2, 8)));

        EnableLosThrottle = config.Bind(
            "AI",
            "EnableLosThrottle",
            true,
            "Throttles AlertRange and LineOfSightDetector checks for distant enemies.");

        AlertRangeInterval = config.Bind(
            "AI",
            "AlertRangeInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant AlertRange.FixedUpdate calls.",
                new AcceptableValueRange<int>(2, 8)));

        LineOfSightInterval = config.Bind(
            "AI",
            "LineOfSightInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant LineOfSightDetector.Update calls.",
                new AcceptableValueRange<int>(2, 8)));

        LosNearHeroDistance = config.Bind(
            "AI",
            "LosNearHeroDistance",
            22f,
            new ConfigDescription(
                "Enemies closer than this to the hero keep full LOS frequency.",
                new AcceptableValueRange<float>(6f, 80f)));

        LosNearCameraDistance = config.Bind(
            "AI",
            "LosNearCameraDistance",
            28f,
            new ConfigDescription(
                "Enemies closer than this to the camera keep full LOS frequency.",
                new AcceptableValueRange<float>(6f, 96f)));

        EnableWalkerThrottle = config.Bind(
            "AI",
            "EnableWalkerThrottle",
            true,
            "Runs Walker.Update less often for distant off-camera walkers.");

        WalkerInterval = config.Bind(
            "AI",
            "WalkerInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant Walker.Update calls.",
                new AcceptableValueRange<int>(2, 8)));

        WalkerNearHeroDistance = config.Bind(
            "AI",
            "WalkerNearHeroDistance",
            26f,
            new ConfigDescription(
                "Walkers closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(8f, 96f)));

        WalkerNearCameraDistance = config.Bind(
            "AI",
            "WalkerNearCameraDistance",
            32f,
            new ConfigDescription(
                "Walkers closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(8f, 128f)));

        EnableFootstepDebounce = config.Bind(
            "Hero",
            "EnableFootstepDebounce",
            true,
            "Suppresses duplicate HeroAudioController footstep state calls when the requested state is already active.");

        EnableWeaverWalkThreadFastPath = config.Bind(
            "Visual",
            "EnableWeaverWalkThreadFastPath",
            true,
            "Replaces WeaverWalkThread.Update with a squared-distance fast path that avoids sqrt outside the alpha falloff band.");

        EnableTk2dQueueDedup = config.Bind(
            "TK2D",
            "EnableTk2dQueueDedup",
            true,
            "Prevents duplicate tk2d text commit queue entries within the same flush window.");

        EnableAraTrailOffscreenCulling = config.Bind(
            "Trails",
            "EnableAraTrailOffscreenCulling",
            true,
            "Skips AraTrail mesh rebuilds when the trail is both off-screen and far from the active camera.");

        AraTrailOffscreenDistance = config.Bind(
            "Trails",
            "AraTrailOffscreenDistance",
            30f,
            new ConfigDescription(
                "Minimum camera distance required before an off-screen AraTrail mesh rebuild can be skipped.",
                new AcceptableValueRange<float>(8f, 160f)));

        EnableNestedFadeMixerThrottle = config.Bind(
            "FadeGroups",
            "EnableNestedFadeMixerThrottle",
            true,
            "Runs distant NestedFadeGroupMixer LateUpdate calls less often during gameplay.");

        NestedFadeMixerInterval = config.Bind(
            "FadeGroups",
            "NestedFadeMixerInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant NestedFadeGroupMixer LateUpdate calls.",
                new AcceptableValueRange<int>(2, 8)));

        NestedFadeMixerNearHeroDistance = config.Bind(
            "FadeGroups",
            "NestedFadeMixerNearHeroDistance",
            20f,
            new ConfigDescription(
                "Mixers closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(6f, 80f)));

        NestedFadeMixerNearCameraDistance = config.Bind(
            "FadeGroups",
            "NestedFadeMixerNearCameraDistance",
            28f,
            new ConfigDescription(
                "Mixers closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(6f, 96f)));

        EnableAmbientFloatThrottle = config.Bind(
            "Ambient",
            "EnableAmbientFloatThrottle",
            true,
            "Runs AmbientFloat.Update less often for distant off-camera decoration objects.");

        AmbientFloatInterval = config.Bind(
            "Ambient",
            "AmbientFloatInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant AmbientFloat.Update calls.",
                new AcceptableValueRange<int>(2, 8)));

        AmbientFloatNearHeroDistance = config.Bind(
            "Ambient",
            "AmbientFloatNearHeroDistance",
            18f,
            new ConfigDescription(
                "AmbientFloat objects closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(4f, 80f)));

        AmbientFloatNearCameraDistance = config.Bind(
            "Ambient",
            "AmbientFloatNearCameraDistance",
            24f,
            new ConfigDescription(
                "AmbientFloat objects closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(4f, 96f)));

        EnableAmbientSwayThrottle = config.Bind(
            "Ambient",
            "EnableAmbientSwayThrottle",
            true,
            "Runs AmbientSway callbacks less often for distant off-camera decoration objects.");

        AmbientSwayInterval = config.Bind(
            "Ambient",
            "AmbientSwayInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant AmbientSway updates.",
                new AcceptableValueRange<int>(2, 8)));

        AmbientSwayNearHeroDistance = config.Bind(
            "Ambient",
            "AmbientSwayNearHeroDistance",
            18f,
            new ConfigDescription(
                "AmbientSway objects closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(4f, 80f)));

        AmbientSwayNearCameraDistance = config.Bind(
            "Ambient",
            "AmbientSwayNearCameraDistance",
            24f,
            new ConfigDescription(
                "AmbientSway objects closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(4f, 96f)));

        EnableDistanceSilhouetteThrottle = config.Bind(
            "Silhouette",
            "EnableDistanceSilhouetteThrottle",
            true,
            "Runs ColourDistanceSilhouette.Update less often for distant off-camera objects.");

        DistanceSilhouetteInterval = config.Bind(
            "Silhouette",
            "DistanceSilhouetteInterval",
            3,
            new ConfigDescription(
                "Frame interval used for distant ColourDistanceSilhouette updates.",
                new AcceptableValueRange<int>(2, 8)));

        DistanceSilhouetteNearHeroDistance = config.Bind(
            "Silhouette",
            "DistanceSilhouetteNearHeroDistance",
            18f,
            new ConfigDescription(
                "Silhouette objects closer than this to the hero keep full update frequency.",
                new AcceptableValueRange<float>(4f, 80f)));

        DistanceSilhouetteNearCameraDistance = config.Bind(
            "Silhouette",
            "DistanceSilhouetteNearCameraDistance",
            24f,
            new ConfigDescription(
                "Silhouette objects closer than this to the camera keep full update frequency.",
                new AcceptableValueRange<float>(4f, 96f)));

        EnableIdlePooledEffectManagerSkip = config.Bind(
            "Effects",
            "EnableIdlePooledEffectManagerSkip",
            true,
            "Skips PooledEffectManager.Update when no pooled effect profiles have pending delayed releases.");

        EnableAggressiveGpuMode = config.Bind(
            "AggressiveGpu",
            "EnableAggressiveGpuMode",
            false,
            "Optional low-spec preset for integrated GPUs and 8 GB systems. Off by default so the game keeps its own video settings.");

        ForceLowAdvancedVideoSettings = config.Bind(
            "AggressiveGpu",
            "ForceLowAdvancedVideoSettings",
            false,
            "When aggressive GPU mode is on, forces low in-game particle, shader, dithering, and noise settings at runtime.");

        ForceLowUnityQualitySettings = config.Bind(
            "AggressiveGpu",
            "ForceLowUnityQualitySettings",
            false,
            "When aggressive GPU mode is on, forces low Unity quality settings such as shadows, anti-aliasing, pixel lights, and HDR/MSAA.");

        DisableVsyncAndFrameCap = config.Bind(
            "AggressiveGpu",
            "DisableVsyncAndFrameCap",
            false,
            "When aggressive GPU mode is on, turns off VSync and removes the runtime frame cap. May cause tearing.");

        EnableBloomLowSpecTuning = config.Bind(
            "AggressiveGpu",
            "EnableBloomLowSpecTuning",
            true,
            "Keeps bloom enabled but clamps it to the cheaper low-spec path when possible.");

        MaxBloomBlurIterations = config.Bind(
            "AggressiveGpu",
            "MaxBloomBlurIterations",
            1,
            new ConfigDescription(
                "Maximum blur iterations allowed for BloomOptimized in low-spec mode.",
                new AcceptableValueRange<int>(1, 2)));

        EnableCameraLowSpecTuning = config.Bind(
            "AggressiveGpu",
            "EnableCameraLowSpecTuning",
            true,
            "Disables camera HDR/MSAA in low-spec modes while preserving the active visual effects stack.");

        EnablePlanarReflectionResolutionReduction = config.Bind(
            "AggressiveGpu",
            "EnablePlanarReflectionResolutionReduction",
            true,
            "Keeps planar reflections enabled but lowers their render texture resolution.");

        PlanarReflectionTextureResolution = config.Bind(
            "AggressiveGpu",
            "PlanarReflectionTextureResolution",
            256,
            new ConfigDescription(
                "Target texture resolution used for planar reflections in low-spec mode.",
                new AcceptableValueRange<int>(128, 1024)));

        EnableUberPostprocessModuleCache = config.Bind(
            "PostFX",
            "EnableUberPostprocessModuleCache",
            true,
            "Caches UberPostprocess module discovery instead of rebuilding the module list every render.");

        UberPostprocessRefreshInterval = config.Bind(
            "PostFX",
            "UberPostprocessRefreshInterval",
            120,
            new ConfigDescription(
                "Frame interval used before refreshing the cached UberPostprocess module list.",
                new AcceptableValueRange<int>(15, 600)));

        EnableLagHitSourceIndex = config.Bind(
            "Combat",
            "EnableLagHitSourceIndex",
            true,
            "Indexes lag-hit sources so CancelAllLagHitsForSource does not need to scan every active HealthManager.");

        EnableDamageEnemiesCooldownFastPath = config.Bind(
            "Combat",
            "EnableDamageEnemiesCooldownFastPath",
            true,
            "Skips DamageEnemies overlap rescans during multihit cooldown frames when no new colliders entered the hitbox.");

        EnableRealtimeReflectionTuning = config.Bind(
            "Reflections",
            "EnableRealtimeReflectionTuning",
            true,
            "Reduces the cost of RealtimeReflections by capping cubemap size and spreading face updates over frames.");

        MaxRealtimeReflectionCubemapSize = config.Bind(
            "Reflections",
            "MaxRealtimeReflectionCubemapSize",
            64,
            new ConfigDescription(
                "Maximum cubemap size for RealtimeReflections.",
                new AcceptableValueRange<int>(32, 256)));

        RealtimeReflectionFrameInterval = config.Bind(
            "Reflections",
            "RealtimeReflectionFrameInterval",
            2,
            new ConfigDescription(
                "Frame interval for RealtimeReflections LateUpdate.",
                new AcceptableValueRange<int>(1, 12)));

        EnableLightBlurTextureReduction = config.Bind(
            "Blur",
            "EnableLightBlurTextureReduction",
            true,
            "Caps LightBlurredBackground render texture height while keeping the blur effect enabled.");

        MaxLightBlurRenderTextureHeight = config.Bind(
            "Blur",
            "MaxLightBlurRenderTextureHeight",
            360,
            new ConfigDescription(
                "Maximum render texture height for LightBlurredBackground.",
                new AcceptableValueRange<int>(180, 720)));

        EnableCameraRenderToMeshThrottle = config.Bind(
            "RenderToTexture",
            "EnableCameraRenderToMeshThrottle",
            true,
            "Runs CameraRenderToMesh LateUpdate less often because the texture is recreated only when dimensions change.");

        CameraRenderToMeshInterval = config.Bind(
            "RenderToTexture",
            "CameraRenderToMeshInterval",
            10,
            new ConfigDescription(
                "Frame interval for CameraRenderToMesh LateUpdate.",
                new AcceptableValueRange<int>(2, 60)));

        EnableAudioEventManagerIdleSkip = config.Bind(
            "Audio",
            "EnableAudioEventManagerIdleSkip",
            true,
            "Skips AudioEventManager.Update when no clip frequency limits are active.");

        EnableLowPassDistanceIdleSkip = config.Bind(
            "Audio",
            "EnableLowPassDistanceIdleSkip",
            true,
            "Skips LowPassDistance.LateUpdate while the source is not playing.");

        EnableAnimatorGroupIdleSkip = config.Bind(
            "Animation",
            "EnableAnimatorGroupIdleSkip",
            true,
            "Skips AnimatorGroup.Update when there are no delayed animator actions pending.");

        EnableAggressiveEffectsThrottle = config.Bind(
            "AggressiveEffects",
            "EnableAggressiveEffectsThrottle",
            true,
            "Aggressively reduces update frequency of selected temporary visual effects and decorative scripts.");

        AggressiveEffectsInterval = config.Bind(
            "AggressiveEffects",
            "AggressiveEffectsInterval",
            2,
            new ConfigDescription(
                "Frame interval used by aggressive effect throttles. Higher values are faster but can make effects less smooth.",
                new AcceptableValueRange<int>(2, 8)));
    }

    public static void SubscribeToChanges()
    {
        EnableGcSmoothing.SettingChanged += OnRuntimeSettingChanged;
        ConvertIdleCleanupToSoftGc.SettingChanged += OnRuntimeSettingChanged;
        IncrementalGcBudgetNanoseconds.SettingChanged += OnRuntimeSettingChanged;
        NonIncrementalGcCooldownSeconds.SettingChanged += OnRuntimeSettingChanged;
        EnableAsyncUploadBufferTuning.SettingChanged += OnRuntimeSettingChanged;
        AsyncUploadBufferSizeMb.SettingChanged += OnRuntimeSettingChanged;
        AsyncUploadTimeSliceMs.SettingChanged += OnRuntimeSettingChanged;
        EnableHighProcessPriority.SettingChanged += OnRuntimeSettingChanged;
        EnableAggressiveGpuMode.SettingChanged += OnRuntimeSettingChanged;
        ForceLowAdvancedVideoSettings.SettingChanged += OnRuntimeSettingChanged;
        ForceLowUnityQualitySettings.SettingChanged += OnRuntimeSettingChanged;
        DisableVsyncAndFrameCap.SettingChanged += OnRuntimeSettingChanged;
        EnableBloomLowSpecTuning.SettingChanged += OnRuntimeSettingChanged;
        MaxBloomBlurIterations.SettingChanged += OnRuntimeSettingChanged;
        EnableCameraLowSpecTuning.SettingChanged += OnRuntimeSettingChanged;
        EnablePlanarReflectionResolutionReduction.SettingChanged += OnRuntimeSettingChanged;
        PlanarReflectionTextureResolution.SettingChanged += OnRuntimeSettingChanged;
    }

    private static void OnRuntimeSettingChanged(object? sender, EventArgs e)
    {
        RuntimeContext.OnConfigurationChanged();
    }
}
