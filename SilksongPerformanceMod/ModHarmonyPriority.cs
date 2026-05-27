namespace SilksongPerformanceMod;

/// <summary>
/// Harmony patch ordering: lower runs earlier (First), higher runs later (Last).
/// </summary>
internal static class ModHarmonyPriority
{
    public const int GcAndCombatFirst = 100;

    public const int Default = 400;

    public const int PostProcessLast = 700;
}
