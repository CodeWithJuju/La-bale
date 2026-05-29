using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class PlanarReflectionPatches
{
    [HarmonyPatch(typeof(PlanarRealtimeReflection), "OnEnable")]
    [HarmonyPostfix]
    private static void Register(PlanarRealtimeReflection __instance)
    {
        PlanarReflectionRegistry.Register(__instance);
        if (RuntimeContext.IsAggressiveGpuModeEnabled())
        {
            RuntimeContext.OptimizePlanarReflection(__instance);
        }
    }

    [HarmonyPatch(typeof(PlanarRealtimeReflection), "OnDisable")]
    [HarmonyPostfix]
    private static void Unregister(PlanarRealtimeReflection __instance)
    {
        PlanarReflectionRegistry.Unregister(__instance);
    }
}
