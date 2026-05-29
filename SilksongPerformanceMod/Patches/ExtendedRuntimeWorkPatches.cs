using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class ExtendedRuntimeWorkPatches
{
    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchExtendedCpuPatches();
    }

    private static readonly AccessTools.FieldRef<AudioEventManager, Dictionary<AudioClip, float>> AudioEventClipReleaseTimesRef =
        AccessTools.FieldRefAccess<AudioEventManager, Dictionary<AudioClip, float>>("clipReleaseTimesLeft");

    private static readonly AccessTools.FieldRef<LowPassDistance, AudioSource> LowPassDistanceAudioSourceRef =
        AccessTools.FieldRefAccess<LowPassDistance, AudioSource>("audioSource");

    private static readonly AccessTools.FieldRef<LowPassDistance, AudioLowPassFilter> LowPassDistanceFilterRef =
        AccessTools.FieldRefAccess<LowPassDistance, AudioLowPassFilter>("lowPassFilter");

    private static readonly FieldInfo AnimatorGroupDelayedPlaysField =
        AccessTools.Field(typeof(AnimatorGroup), "delayedPlays");

    [HarmonyPatch(typeof(RealtimeReflections), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleRealtimeReflections(RealtimeReflections __instance)
    {
        if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() || !ModSettings.EnableRealtimeReflectionTuning.Value)
        {
            return true;
        }

        if (!__instance.oneFacePerFrame)
        {
            __instance.oneFacePerFrame = true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(
            __instance.GetInstanceID(),
            ModSettings.RealtimeReflectionFrameInterval.Value);
    }

    [HarmonyPatch(typeof(CameraRenderToMesh), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleCameraRenderToMesh(CameraRenderToMesh __instance)
    {
        if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() || !ModSettings.EnableCameraRenderToMeshThrottle.Value)
        {
            return true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(
            __instance.GetInstanceID(),
            ModSettings.CameraRenderToMeshInterval.Value);
    }

    [HarmonyPatch(typeof(AudioEventManager), "Update")]
    [HarmonyPrefix]
    private static bool SkipIdleAudioEventManager(AudioEventManager __instance)
    {
        if (!ModSettings.EnableAudioEventManagerIdleSkip.Value)
        {
            return true;
        }

        Dictionary<AudioClip, float>? releaseTimes = AudioEventClipReleaseTimesRef(__instance);
        if (releaseTimes == null)
        {
            return true;
        }

        return releaseTimes.Count > 0;
    }

    [HarmonyPatch(typeof(LowPassDistance), "LateUpdate")]
    [HarmonyPrefix]
    private static bool SkipInactiveLowPassDistance(LowPassDistance __instance)
    {
        if (!ModSettings.EnableLowPassDistanceIdleSkip.Value)
        {
            return true;
        }

        AudioSource? audioSource = LowPassDistanceAudioSourceRef(__instance);
        AudioLowPassFilter? lowPassFilter = LowPassDistanceFilterRef(__instance);
        if (audioSource == null || lowPassFilter == null)
        {
            return true;
        }

        return audioSource.isPlaying && audioSource.enabled && lowPassFilter.enabled;
    }

    [HarmonyPatch(typeof(AnimatorGroup), "Update")]
    [HarmonyPrefix]
    private static bool SkipIdleAnimatorGroup(AnimatorGroup __instance)
    {
        if (!ModSettings.EnableAnimatorGroupIdleSkip.Value)
        {
            return true;
        }

        object? raw = AnimatorGroupDelayedPlaysField?.GetValue(__instance);
        if (raw == null)
        {
            return false;
        }

        if (raw is IDictionary dict)
        {
            return dict.Count > 0;
        }

        return true;
    }
}
