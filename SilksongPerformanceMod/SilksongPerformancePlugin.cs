using BepInEx;
using HarmonyLib;

namespace SilksongPerformanceMod;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public sealed class SilksongPerformancePlugin : BaseUnityPlugin
{
    private Harmony? _harmony;

    private void Awake()
    {
        RuntimeContext.Initialize(Logger);
        ModSettings.Bind(Config);
        ModSettings.SubscribeToChanges();

        _harmony = new Harmony(PluginInfo.Guid);
        _harmony.PatchAll();
        SceneMemoryCoordinator.Hook();
        OffscreenCullingCoordinator.RefreshInstallation();

        RuntimeContext.ApplyImmediateRuntimeOverrides();
        Logger.LogInfo(
            $"{PluginInfo.Name} {PluginInfo.Version} loaded | " +
            $"highFpsMode={ModSettings.EnableHighFpsCompatibilityMode.Value}, " +
            $"perFrameThrottles={ModSettings.EnablePerFrameHarmonyThrottles.Value}, " +
            $"offscreenCulling={ModSettings.EnableAggressiveOffscreenCulling.Value}");
    }

    private void OnDestroy()
    {
        OffscreenCullingCoordinator.Uninstall();
        SceneMemoryCoordinator.Unhook();
        _harmony?.UnpatchSelf();
        RuntimeContext.ResetRuntimeOverrides();
    }
}
