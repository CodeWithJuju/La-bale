using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class PooledEffectManagerIdleSkipPatches
{
    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchExtendedCpuPatches() &&
               ModSettings.EnableIdlePooledEffectManagerSkip.Value;
    }

    [HarmonyPatch(typeof(PooledEffectManager), "Update")]
    [HarmonyPrefix]
    private static bool SkipIdlePooledEffectManagerUpdate()
    {
        return PooledEffectPendingCounter.Pending > 0;
    }

    [HarmonyPatch(typeof(PooledEffectTracker<PooledEffect>), nameof(PooledEffectTracker<PooledEffect>.EnqueueRelease))]
    [HarmonyPostfix]
    private static void TrackEnqueueRelease(bool __result)
    {
        if (__result)
        {
            PooledEffectPendingCounter.Increment();
        }
    }

    [HarmonyPatch(typeof(PooledEffectTracker<PooledEffect>), nameof(PooledEffectTracker<PooledEffect>.ReleaseEffect))]
    [HarmonyPrefix]
    private static void TrackReleaseEffect()
    {
        PooledEffectPendingCounter.Decrement();
    }
}
