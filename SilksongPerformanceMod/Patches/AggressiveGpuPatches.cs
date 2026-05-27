using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class AggressiveGpuPatches
{
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.ApplyEffectConfiguration))]
    [HarmonyPostfix]
    private static void ReapplyAggressiveGpuModeAfterCameraConfig()
    {
        RuntimeContext.ApplyAggressiveGpuMode("CameraController.ApplyEffectConfiguration");
    }

    [HarmonyPatch(typeof(GameCameras), nameof(GameCameras.SceneInit))]
    [HarmonyPostfix]
    private static void ReapplyAggressiveGpuModeAfterSceneInit()
    {
        RuntimeContext.ApplyAggressiveGpuMode("GameCameras.SceneInit");
    }
}
