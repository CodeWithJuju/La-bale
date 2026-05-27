using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class LagHitCleanupPatches
{
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), typeof(UnityEngine.Object))]
    [HarmonyPostfix]
    [HarmonyPriority(ModHarmonyPriority.Default)]
    private static void CleanupLagHitSourceOnDestroy(Object obj)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value || obj is not GameObject gameObject)
        {
            return;
        }

        LagHitRegistry.UnregisterSource(gameObject);
    }
}
