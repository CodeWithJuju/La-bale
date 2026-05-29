using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class PostFxAndParticleLifecyclePatches
{
    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchExtendedCpuPatches();
    }

    private static readonly AccessTools.FieldRef<ColorCurvesManager, List<Keyframe[]>> RedPairedKeyframesRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, List<Keyframe[]>>("RedPairedKeyframes");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, List<Keyframe[]>> GreenPairedKeyframesRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, List<Keyframe[]>>("GreenPairedKeyframes");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, List<Keyframe[]>> BluePairedKeyframesRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, List<Keyframe[]>>("BluePairedKeyframes");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, bool> ColorCurvesChangesInEditorRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, bool>("ChangesInEditor");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, float> LastFactorRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, float>("LastFactor");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, float> LastSaturationARef =
        AccessTools.FieldRefAccess<ColorCurvesManager, float>("LastSaturationA");

    private static readonly AccessTools.FieldRef<ColorCurvesManager, float> LastSaturationBRef =
        AccessTools.FieldRefAccess<ColorCurvesManager, float>("LastSaturationB");

    private static readonly AccessTools.FieldRef<DisableParticleCollisonDelay, float> ParticleCollisionDelayRef =
        AccessTools.FieldRefAccess<DisableParticleCollisonDelay, float>("delay");

    private static readonly AccessTools.FieldRef<DisableParticleCollisonDelay, float> ParticleCollisionTimerRef =
        AccessTools.FieldRefAccess<DisableParticleCollisonDelay, float>("timer");

    private static readonly AccessTools.FieldRef<DisableParticleCollisonDelay, bool> ParticleCollisionPlayedRef =
        AccessTools.FieldRefAccess<DisableParticleCollisonDelay, bool>("played");

    private static readonly AccessTools.FieldRef<DisableParticleCollisonDelay, bool> ParticleCollisionDidEndRef =
        AccessTools.FieldRefAccess<DisableParticleCollisonDelay, bool>("didCollisionEnd");

    private static readonly AccessTools.FieldRef<DisableParticleCollisonDelay, ParticleSystem> ParticleCollisionSystemRef =
        AccessTools.FieldRefAccess<DisableParticleCollisonDelay, ParticleSystem>("particle_system");

    [HarmonyPatch(typeof(ColorCurvesManager), "Update")]
    [HarmonyPrefix]
    private static bool SkipIdleColorCurvesManager(ColorCurvesManager __instance)
    {
        if (!ModSettings.EnableColorCurvesManagerIdleSkip.Value)
        {
            return true;
        }

        if (ColorCurvesChangesInEditorRef(__instance))
        {
            return true;
        }

        if (RedPairedKeyframesRef(__instance) == null ||
            GreenPairedKeyframesRef(__instance) == null ||
            BluePairedKeyframesRef(__instance) == null)
        {
            return true;
        }

        return __instance.Factor != LastFactorRef(__instance) ||
               __instance.SaturationA != LastSaturationARef(__instance) ||
               __instance.SaturationB != LastSaturationBRef(__instance);
    }

    [HarmonyPatch(typeof(ParticleSystemAutoRecycle), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleParticleSystemAutoRecycle(ParticleSystemAutoRecycle __instance)
    {
        return ShouldRunParticleLifecyclePoll(__instance.GetInstanceID());
    }

    [HarmonyPatch(typeof(ParticleSystemAutoDestroy), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleParticleSystemAutoDestroy(ParticleSystemAutoDestroy __instance)
    {
        return ShouldRunParticleLifecyclePoll(__instance.GetInstanceID() + 17);
    }

    [HarmonyPatch(typeof(ParticleSystemAutoDeactivate), "Update")]
    [HarmonyPrefix]
    private static bool ThrottleParticleSystemAutoDeactivate(ParticleSystemAutoDeactivate __instance)
    {
        return ShouldRunParticleLifecyclePoll(__instance.GetInstanceID() + 31);
    }

    [HarmonyPatch(typeof(DisableParticleCollisonDelay), "Update")]
    [HarmonyPrefix]
    private static bool FixParticleCollisionDelay(DisableParticleCollisonDelay __instance)
    {
        if (!ModSettings.EnableParticleCollisionDelayFix.Value)
        {
            return true;
        }

        if (ParticleCollisionDidEndRef(__instance))
        {
            return false;
        }

        ParticleSystem particleSystem = ParticleCollisionSystemRef(__instance);
        if (particleSystem == null)
        {
            return true;
        }

        if (!ParticleCollisionPlayedRef(__instance) && particleSystem.IsAlive())
        {
            ParticleCollisionPlayedRef(__instance) = true;
        }

        if (!ParticleCollisionPlayedRef(__instance))
        {
            return false;
        }

        ParticleCollisionTimerRef(__instance) += Time.deltaTime;
        if (ParticleCollisionTimerRef(__instance) >= ParticleCollisionDelayRef(__instance))
        {
            ParticleSystem.CollisionModule collision = particleSystem.collision;
            collision.enabled = false;
            ParticleCollisionDidEndRef(__instance) = true;
        }

        return false;
    }

    private static bool ShouldRunParticleLifecyclePoll(int seed)
    {
        if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() ||
            !ModSettings.EnableParticleAutoLifecycleThrottle.Value ||
            !RuntimeContext.ShouldApplyDistanceThrottle())
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(seed, ModSettings.ParticleAutoLifecycleInterval.Value);
    }
}
