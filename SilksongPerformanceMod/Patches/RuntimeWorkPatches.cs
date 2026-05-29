using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

/// <summary>
/// One-shot / OnEnable tuning patches kept even in High FPS mode (no per-frame hook).
/// </summary>
[HarmonyPatch]
internal static class RuntimeWorkPatches
{
    private static readonly AccessTools.FieldRef<LightBlurredBackground, int> LightBlurRenderTextureHeightRef =
        AccessTools.FieldRefAccess<LightBlurredBackground, int>("renderTextureHeight");

    [HarmonyPatch(typeof(RealtimeReflections), "Start")]
    [HarmonyPrefix]
    private static void TuneRealtimeReflectionsBeforeStart(RealtimeReflections __instance)
    {
        if (!ModSettings.EnableRealtimeReflectionTuning.Value)
        {
            return;
        }

        __instance.oneFacePerFrame = true;
        __instance.cubemapSize = Mathf.Min(
            __instance.cubemapSize,
            ModSettings.MaxRealtimeReflectionCubemapSize.Value);
    }

    [HarmonyPatch(typeof(LightBlurredBackground), "OnEnable")]
    [HarmonyPrefix]
    private static void ReduceLightBlurTextureHeight(LightBlurredBackground __instance)
    {
        if (!ModSettings.EnableLightBlurTextureReduction.Value)
        {
            return;
        }

        ref int currentHeight = ref LightBlurRenderTextureHeightRef(__instance);
        currentHeight = Mathf.Min(currentHeight, ModSettings.MaxLightBlurRenderTextureHeight.Value);
    }
}
