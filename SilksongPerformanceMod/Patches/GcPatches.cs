using HarmonyLib;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
[HarmonyPriority(ModHarmonyPriority.GcAndCombatFirst)]
internal static class GcPatches
{
    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchGcInterception();
    }

    [HarmonyPatch(typeof(GCManager), "Awake")]
    [HarmonyPrefix]
    private static bool SkipGcManagerAwake()
    {
        if (!ModSettings.EnableGcSmoothing.Value)
        {
            return true;
        }

        RuntimeContext.TryEnableAutomaticGc("GCManager.Awake");
        return false;
    }

    [HarmonyPatch(typeof(GCManager), "Update")]
    [HarmonyPrefix]
    private static bool SkipGcManagerUpdate()
    {
        return !ModSettings.EnableGcSmoothing.Value;
    }

    [HarmonyPatch(typeof(GCManager), nameof(GCManager.Collect))]
    [HarmonyPrefix]
    private static bool InterceptCollect()
    {
        if (!ModSettings.EnableGcSmoothing.Value)
        {
            return true;
        }

        if (RuntimeContext.AllowBlockingGc())
        {
            return true;
        }

        RuntimeContext.PerformSoftGc("GCManager.Collect");
        return false;
    }

    [HarmonyPatch(typeof(GCManager), nameof(GCManager.ForceCollect))]
    [HarmonyPrefix]
    private static bool InterceptForceCollect(ref bool blocking, ref bool compacting)
    {
        if (!ModSettings.EnableGcSmoothing.Value)
        {
            return true;
        }

        if (!RuntimeContext.AllowBlockingGc())
        {
            RuntimeContext.PerformSoftGc("GCManager.ForceCollect");
            return false;
        }

        RuntimeContext.TryEnableAutomaticGc("GCManager.ForceCollect");
        blocking = false;
        compacting = false;
        return true;
    }

    [HarmonyPatch(typeof(IdleMemoryCleaner), "CleanMemory")]
    [HarmonyPrefix]
    private static bool SoftenIdleCleaner()
    {
        if (!ModSettings.ConvertIdleCleanupToSoftGc.Value)
        {
            return true;
        }

        RuntimeContext.PerformSoftGc("IdleMemoryCleaner.CleanMemory");
        return false;
    }
}
