namespace SilksongPerformanceMod;

/// <summary>
/// Harmony Prepare helpers. When Prepare returns false, the patch is never injected
/// (zero per-frame overhead), unlike a prefix that returns true immediately.
/// </summary>
internal static class ModHarmonyPrepare
{
    public static bool ShouldPatchPerFrameThrottles()
    {
        return !ModSettings.EnableHighFpsCompatibilityMode.Value &&
               ModSettings.EnablePerFrameHarmonyThrottles.Value;
    }

    public static bool ShouldPatchExtendedCpuPatches()
    {
        return !ModSettings.EnableHighFpsCompatibilityMode.Value;
    }

    public static bool ShouldPatchGcInterception()
    {
        return ModSettings.EnableGcSmoothing.Value || ModSettings.ConvertIdleCleanupToSoftGc.Value;
    }
}
