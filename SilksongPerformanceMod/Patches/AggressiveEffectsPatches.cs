using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class AggressiveEffectsPatches
{
    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchPerFrameThrottles() &&
               ModSettings.EnableAggressiveEffectsThrottle.Value;
    }

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

    [HarmonyPatch(typeof(CycloneDust), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleCycloneDust(CycloneDust __instance)
    {
        return ShouldRun(__instance.GetInstanceID() + 37);
    }

    private static bool ShouldRun(int seed)
    {
        if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() ||
            !ModSettings.EnableAggressiveEffectsThrottle.Value ||
            !RuntimeContext.ShouldApplyDistanceThrottle())
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(seed, ModSettings.AggressiveEffectsInterval.Value);
    }
}
