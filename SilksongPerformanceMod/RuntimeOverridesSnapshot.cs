using GlobalEnums;
using UnityEngine;

namespace SilksongPerformanceMod;

/// <summary>
/// Captures Unity / game video settings before runtime overrides so teardown can restore them.
/// </summary>
internal sealed class RuntimeOverridesSnapshot
{
    private bool _captured;

    private int _vSyncCount;

    private int _targetFrameRate;

    private int _antiAliasing;

    private int _pixelLightCount;

    private ShadowQuality _shadows;

    private float _shadowDistance;

    private bool _realtimeReflectionProbes;

    private AnisotropicFiltering _anisotropicFiltering;

    private bool _softParticles;

    private int? _asyncUploadBufferSize;

    private int? _asyncUploadTimeSlice;

    private bool _gameVideoCaptured;

    private int _gameVSync;

    private int _gameTargetFrameRate;

    private int _gameParticleEffectsLevel;

    private ShaderQualities _gameShaderQuality;

    private bool _gameCameraNoise;

    public void CaptureIfNeeded()
    {
        if (_captured)
        {
            return;
        }

        _captured = true;
        _vSyncCount = QualitySettings.vSyncCount;
        _targetFrameRate = Application.targetFrameRate;
        _antiAliasing = QualitySettings.antiAliasing;
        _pixelLightCount = QualitySettings.pixelLightCount;
        _shadows = QualitySettings.shadows;
        _shadowDistance = QualitySettings.shadowDistance;
        _realtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
        _anisotropicFiltering = QualitySettings.anisotropicFiltering;
        _softParticles = QualitySettings.softParticles;

        if (ModSettings.EnableAsyncUploadBufferTuning.Value)
        {
            _asyncUploadBufferSize = RuntimeContext.TryReadAsyncUploadBufferSize();
            _asyncUploadTimeSlice = RuntimeContext.TryReadAsyncUploadTimeSlice();
        }

        GameManager? gm = GameManager.instance;
        GameSettings? settings = gm?.gameSettings;
        if (settings != null)
        {
            _gameVideoCaptured = true;
            _gameVSync = settings.vSync;
            _gameTargetFrameRate = settings.targetFrameRate;
            _gameParticleEffectsLevel = settings.particleEffectsLevel;
            _gameShaderQuality = settings.shaderQuality;
            _gameCameraNoise = settings.cameraNoise;
        }
    }

    public void Restore()
    {
        if (!_captured)
        {
            return;
        }

        QualitySettings.vSyncCount = _vSyncCount;
        Application.targetFrameRate = _targetFrameRate;
        QualitySettings.antiAliasing = _antiAliasing;
        QualitySettings.pixelLightCount = _pixelLightCount;
        QualitySettings.shadows = _shadows;
        QualitySettings.shadowDistance = _shadowDistance;
        QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbes;
        QualitySettings.anisotropicFiltering = _anisotropicFiltering;
        QualitySettings.softParticles = _softParticles;

        if (_asyncUploadBufferSize.HasValue)
        {
            RuntimeContext.TryWriteAsyncUploadBufferSize(_asyncUploadBufferSize.Value);
        }

        if (_asyncUploadTimeSlice.HasValue)
        {
            RuntimeContext.TryWriteAsyncUploadTimeSlice(_asyncUploadTimeSlice.Value);
        }

        if (_gameVideoCaptured)
        {
            GameManager? gm = GameManager.instance;
            GameSettings? settings = gm?.gameSettings;
            if (settings != null && gm != null)
            {
                settings.vSync = _gameVSync;
                settings.targetFrameRate = _gameTargetFrameRate;
                settings.particleEffectsLevel = _gameParticleEffectsLevel;
                settings.shaderQuality = _gameShaderQuality;
                settings.cameraNoise = _gameCameraNoise;
                gm.RefreshParticleSystems();
            }
        }

        GCManager.DisabledManualCollect = false;
        _captured = false;
    }
}
