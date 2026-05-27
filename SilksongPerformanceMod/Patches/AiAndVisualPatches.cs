using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class AiAndVisualPatches
{
    [HarmonyPatch(typeof(ParticleCulling), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleParticleCulling(ParticleCulling __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableParticleCullingThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.ParticleNearHeroDistance.Value,
            ModSettings.ParticleNearCameraDistance.Value,
            ModSettings.ParticleCullingInterval.Value);
    }

    [HarmonyPatch(typeof(WaterfallParticles), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleWaterfallParticles(WaterfallParticles __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableWaterfallParticleThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.WaterfallParticleNearHeroDistance.Value,
            ModSettings.WaterfallParticleNearCameraDistance.Value,
            ModSettings.WaterfallParticleInterval.Value);
    }

    [HarmonyPatch(typeof(ParticleSystemAutoDisable), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleParticleAutoDisable(ParticleSystemAutoDisable __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableParticleAutoDisableThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(
            __instance.GetInstanceID(),
            ModSettings.ParticleAutoDisableInterval.Value);
    }

    [HarmonyPatch(typeof(SceneColorManager), nameof(SceneColorManager.UpdateScript))]
    [HarmonyPrefix]
    private static bool ThrottleSceneColor(SceneColorManager __instance, bool forceUpdate = false)
    {
        if (!ModSettings.EnableSceneColorThrottle.Value || forceUpdate)
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(__instance.GetInstanceID(), ModSettings.SceneColorInterval.Value);
    }

    [HarmonyPatch(typeof(AlertRange), "FixedUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleAlertRange(AlertRange __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableLosThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.LosNearHeroDistance.Value,
            ModSettings.LosNearCameraDistance.Value,
            ModSettings.AlertRangeInterval.Value);
    }

    [HarmonyPatch(typeof(LineOfSightDetector), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleLineOfSight(LineOfSightDetector __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableLosThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.LosNearHeroDistance.Value,
            ModSettings.LosNearCameraDistance.Value,
            ModSettings.LineOfSightInterval.Value);
    }

    [HarmonyPatch(typeof(Walker), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleWalker(Walker __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableWalkerThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.WalkerNearHeroDistance.Value,
            ModSettings.WalkerNearCameraDistance.Value,
            ModSettings.WalkerInterval.Value);
    }

    // REMOVIDO: ThrottleAmbientFloat
    // AmbientFloat já possui fpsLimit nativo com nextUpdateTime — o patch anterior
    // adicionava overhead do Harmony toda frame sem nenhum benefício extra.

    // REMOVIDO: ThrottleAmbientSway
    // AmbientSway já possui rate limiting via profile.Fps e usa um singleton
    // centralizado (AmbientSwayCallbackHooks) — patchear OnUpdate duplicava
    // o custo sem reduzir o trabalho real.

    [HarmonyPatch(typeof(ColourDistanceSilhouette), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleDistanceSilhouette(ColourDistanceSilhouette __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableDistanceSilhouetteThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.DistanceSilhouetteNearHeroDistance.Value,
            ModSettings.DistanceSilhouetteNearCameraDistance.Value,
            ModSettings.DistanceSilhouetteInterval.Value);
    }
}