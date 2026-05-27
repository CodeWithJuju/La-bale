using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class EffectManagerPatches
{
    [HarmonyPatch(typeof(PooledEffectManager), "Update")]
    [HarmonyPrefix]
    private static bool SkipIdlePooledEffectManagerUpdate()
    {
        if (!ModSettings.EnableIdlePooledEffectManagerSkip.Value)
        {
            return true;
        }

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
