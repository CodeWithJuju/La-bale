using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class AggressiveEffectsPatches
{
    [HarmonyPatch(typeof(HardLandEffect), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleHardLandEffect(HardLandEffect __instance)
    {
        return ShouldRun(__instance.GetInstanceID());
    }

    [HarmonyPatch(typeof(SoftLandEffect), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleSoftLandEffect(SoftLandEffect __instance)
    {
        return ShouldRun(__instance.GetInstanceID() + 11);
    }

    [HarmonyPatch(typeof(JumpEffects), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleJumpEffects(JumpEffects __instance)
    {
        return ShouldRun(__instance.GetInstanceID() + 23);
    }

    [HarmonyPatch(typeof(CycloneDust), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleCycloneDust(CycloneDust __instance)
    {
        return ShouldRun(__instance.GetInstanceID() + 37);
    }

    [HarmonyPatch(typeof(CameraBlurPlaneAnimator), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleCameraBlurPlaneAnimator(CameraBlurPlaneAnimator __instance)
    {
        return ShouldRun(__instance.GetInstanceID() + 53);
    }

    private static bool ShouldRun(int seed)
    {
        if (!ModSettings.EnableAggressiveEffectsThrottle.Value || !RuntimeContext.ShouldApplyDistanceThrottle())
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(seed, ModSettings.AggressiveEffectsInterval.Value);
    }
}
