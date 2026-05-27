using GlobalEnums;
using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class HeroAudioPatches
{
    private static readonly AccessTools.FieldRef<HeroAudioController, bool> CanPlayWalkRef =
        AccessTools.FieldRefAccess<HeroAudioController, bool>("canPlayWalk");

    private static readonly AccessTools.FieldRef<HeroAudioController, bool> CanPlayRunRef =
        AccessTools.FieldRefAccess<HeroAudioController, bool>("canPlayRun");

    private static readonly AccessTools.FieldRef<HeroAudioController, bool> CanPlaySprintRef =
        AccessTools.FieldRefAccess<HeroAudioController, bool>("canPlaySprint");

    [HarmonyPatch(typeof(HeroAudioController), nameof(HeroAudioController.PlaySound))]
    [HarmonyPrefix]
    private static bool DebounceFootstepPlay(HeroAudioController __instance, HeroSounds soundEffect)
    {
        if (!ModSettings.EnableFootstepDebounce.Value)
        {
            return true;
        }

        return soundEffect switch
        {
            HeroSounds.FOOTSTEPS_WALK => !CanPlayWalkRef(__instance),
            HeroSounds.FOOTSTEPS_RUN => !CanPlayRunRef(__instance),
            HeroSounds.FOOTSTEPS_SPRINT => !CanPlaySprintRef(__instance),
            _ => true
        };
    }

    [HarmonyPatch(typeof(HeroAudioController), nameof(HeroAudioController.StopSound))]
    [HarmonyPrefix]
    private static bool DebounceFootstepStop(HeroAudioController __instance, HeroSounds soundEffect)
    {
        if (!ModSettings.EnableFootstepDebounce.Value)
        {
            return true;
        }

        return soundEffect switch
        {
            HeroSounds.FOOTSTEPS_WALK => CanPlayWalkRef(__instance),
            HeroSounds.FOOTSTEPS_RUN => CanPlayRunRef(__instance),
            HeroSounds.FOOTSTEPS_SPRINT => CanPlaySprintRef(__instance),
            _ => true
        };
    }
}
