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

    [HarmonyPatch(typeof(AmbientFloat), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleAmbientFloat(AmbientFloat __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableAmbientFloatThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.AmbientFloatNearHeroDistance.Value,
            ModSettings.AmbientFloatNearCameraDistance.Value,
            ModSettings.AmbientFloatInterval.Value);
    }

    [HarmonyPatch(typeof(AmbientSway), "OnUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleAmbientSway(AmbientSway __instance)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableAmbientSwayThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunForDistance(
            __instance.transform,
            ModSettings.AmbientSwayNearHeroDistance.Value,
            ModSettings.AmbientSwayNearCameraDistance.Value,
            ModSettings.AmbientSwayInterval.Value);
    }

    [HarmonyPatch(typeof(FloatingObject), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleFloatingObject(FloatingObject __instance)
    {
        return ShouldThrottleDecorativeUpdate(__instance.transform, __instance.GetInstanceID());
    }

    [HarmonyPatch(typeof(JitterSelfSimple), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleJitterSelfSimple(JitterSelfSimple __instance)
    {
        return ShouldThrottleDecorativeUpdate(__instance.transform, __instance.GetInstanceID() + 17);
    }

    [HarmonyPatch(typeof(SpriteFadePulse), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleSpriteFadePulse(SpriteFadePulse __instance)
    {
        return ShouldThrottleDecorativeUpdate(__instance.transform, __instance.GetInstanceID() + 29);
    }

    [HarmonyPatch(typeof(TK2DSpriteFadePulse), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleTk2dSpriteFadePulse(TK2DSpriteFadePulse __instance)
    {
        return ShouldThrottleDecorativeUpdate(__instance.transform, __instance.GetInstanceID() + 43);
    }

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

    private static bool ShouldThrottleDecorativeUpdate(Transform transform, int seed)
    {
        if (!RuntimeContext.ShouldApplyDistanceThrottle() || !ModSettings.EnableAggressiveOffscreenCulling.Value)
        {
            return true;
        }

        if (!RuntimeContext.ShouldRunForDistance(
                transform,
                ModSettings.AmbientFloatNearHeroDistance.Value,
                ModSettings.AmbientFloatNearCameraDistance.Value,
                ModSettings.AmbientFloatInterval.Value))
        {
            return false;
        }

        return RuntimeContext.ShouldRunEveryNFrames(seed, ModSettings.AmbientSwayInterval.Value);
    }
}