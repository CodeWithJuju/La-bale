using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class RuntimeWorkPatches
{
    private static readonly AccessTools.FieldRef<LightBlurredBackground, int> LightBlurRenderTextureHeightRef =
        AccessTools.FieldRefAccess<LightBlurredBackground, int>("renderTextureHeight");

    private static readonly AccessTools.FieldRef<AudioEventManager, Dictionary<AudioClip, float>> AudioEventClipReleaseTimesRef =
        AccessTools.FieldRefAccess<AudioEventManager, Dictionary<AudioClip, float>>("clipReleaseTimesLeft");

    private static readonly AccessTools.FieldRef<LowPassDistance, AudioSource> LowPassDistanceAudioSourceRef =
        AccessTools.FieldRefAccess<LowPassDistance, AudioSource>("audioSource");

    private static readonly AccessTools.FieldRef<LowPassDistance, AudioLowPassFilter> LowPassDistanceFilterRef =
        AccessTools.FieldRefAccess<LowPassDistance, AudioLowPassFilter>("lowPassFilter");

    // CORRIGIDO: trocado de FieldInfo.GetValue (boxing + reflection toda frame)
    // para FieldRefAccess (acesso direto equivalente a um ponteiro gerenciado).
    // O tipo exato precisa corresponder ao campo "delayedPlays" em AnimatorGroup —
    // ajuste o tipo genérico se o jogo usar uma coleção diferente.
    private static readonly AccessTools.FieldRef<AnimatorGroup, IDictionary> AnimatorGroupDelayedPlaysRef =
        AccessTools.FieldRefAccess<AnimatorGroup, IDictionary>("delayedPlays");

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

    [HarmonyPatch(typeof(RealtimeReflections), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleRealtimeReflections(RealtimeReflections __instance)
    {
        if (!ModSettings.EnableRealtimeReflectionTuning.Value)
        {
            return true;
        }

        // CORRIGIDO: setter guard — evita escrever a propriedade toda frame
        // quando o valor já é true, prevenindo chamadas redundantes à Unity API.
        if (!__instance.oneFacePerFrame)
        {
            __instance.oneFacePerFrame = true;
        }

        return RuntimeContext.ShouldRunEveryNFrames(
            __instance.GetInstanceID(),
            ModSettings.RealtimeReflectionFrameInterval.Value);
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

    [HarmonyPatch(typeof(CameraRenderToMesh), "LateUpdate")]
    [HarmonyPrefix]
    private static bool ThrottleCameraRenderToMesh(CameraRenderToMesh __instance)
    {
        if (!ModSettings.EnableCameraRenderToMeshThrottle.Value)
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

        // CORRIGIDO: acesso via FieldRef (sem boxing, sem alocação).
        // Se o jogo lançar InvalidCastException aqui, inspecione o tipo real
        // de "delayedPlays" com dnSpy/ILSpy e ajuste o tipo genérico do FieldRef.
        IDictionary? delayedPlays = AnimatorGroupDelayedPlaysRef(__instance);
        if (delayedPlays == null)
        {
            return false;
        }

        foreach (object value in delayedPlays.Values)
        {
            if (value is ICollection collection && collection.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
}