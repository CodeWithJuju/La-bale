using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
[HarmonyPriority(ModHarmonyPriority.GcAndCombatFirst)]
internal static class CombatAndPostFxPatches
{
    private static readonly AccessTools.FieldRef<DamageEnemies, int> DamageEnemiesStepsToNextHitRef =
        AccessTools.FieldRefAccess<DamageEnemies, int>("stepsToNextHit");

    private static readonly AccessTools.FieldRef<DamageEnemies, HashSet<Collider2D>> DamageEnemiesFrameQueueRef =
        AccessTools.FieldRefAccess<DamageEnemies, HashSet<Collider2D>>("frameQueue");

    private static readonly FieldInfo HealthManagerRunningLagHitsField =
        AccessTools.Field(typeof(HealthManager), "runningLagHits");

    private static readonly List<HealthManager> LagHitSnapshot = new();

    private static bool _lagHitIndexDirty;

    private static int _lastLagHitPruneFrame;

    [HarmonyPatch(typeof(DamageEnemies), "EvaluateDamage")]
    [HarmonyPrefix]
    private static bool FastSkipDamageEnemiesCooldownScan(DamageEnemies __instance, ref bool __result)
    {
        if (!ModSettings.EnableDamageEnemiesCooldownFastPath.Value || !__instance.multiHitter)
        {
            return true;
        }

        if (GetStepsToNextHit(__instance) <= 0)
        {
            return true;
        }

        if (GetFrameQueue(__instance).Count > 0)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.DoLagHits))]
    [HarmonyPrefix]
    private static void CaptureLagHitCountBeforeStart(HealthManager __instance, out int __state)
    {
        __state = GetRunningLagHitsCount(__instance);
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.DoLagHits))]
    [HarmonyPostfix]
    private static void RegisterLagHitSource(HealthManager __instance, HitInstance hitInstance, int __state)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value || hitInstance.Source == null)
        {
            return;
        }

        if (GetRunningLagHitsCount(__instance) > __state)
        {
            LagHitRegistry.Register(hitInstance.Source, __instance);
        }
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.CancelAllLagHitsForSource))]
    [HarmonyPrefix]
    private static bool OptimizeCancelAllLagHitsForSource(GameObject source)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value || source == null)
        {
            return true;
        }

        if (_lagHitIndexDirty || Time.frameCount - _lastLagHitPruneFrame >= 300)
        {
            LagHitRegistry.PruneAllStaleEntries();
            _lagHitIndexDirty = false;
            _lastLagHitPruneFrame = Time.frameCount;
        }

        if (!LagHitRegistry.TryCopySnapshot(source, LagHitSnapshot))
        {
            return true;
        }

        try
        {
            for (int i = 0; i < LagHitSnapshot.Count; i++)
            {
                HealthManager? healthManager = LagHitSnapshot[i];
                if (healthManager == null)
                {
                    continue;
                }

                healthManager.CancelLagHitsForSource(source);
            }
        }
        finally
        {
            LagHitSnapshot.Clear();
        }

        LagHitRegistry.PruneSource(source);
        return false;
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.CancelLagHitsForSource))]
    [HarmonyPostfix]
    private static void UnregisterLagHitSource(HealthManager __instance, GameObject source)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value || source == null)
        {
            return;
        }

        LagHitRegistry.Unregister(source, __instance);
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.CancelAllLagHits))]
    [HarmonyPostfix]
    private static void UnregisterAllLagHitSources(HealthManager __instance)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value)
        {
            return;
        }

        LagHitRegistry.UnregisterAll(__instance);
        _lagHitIndexDirty = true;
    }

    [HarmonyPatch(typeof(HealthManager), "OnDisable")]
    [HarmonyPostfix]
    private static void RemoveDisabledHealthManagerFromLagHitIndex(HealthManager __instance)
    {
        if (!ModSettings.EnableLagHitSourceIndex.Value)
        {
            return;
        }

        LagHitRegistry.UnregisterAll(__instance);
        _lagHitIndexDirty = true;
    }

    private static int GetStepsToNextHit(DamageEnemies damageEnemies)
    {
        return DamageEnemiesStepsToNextHitRef(damageEnemies);
    }

    private static HashSet<Collider2D> GetFrameQueue(DamageEnemies damageEnemies)
    {
        return DamageEnemiesFrameQueueRef(damageEnemies);
    }

    private static int GetRunningLagHitsCount(HealthManager healthManager)
    {
        if (HealthManagerRunningLagHitsField.GetValue(healthManager) is not System.Collections.ICollection lagHits)
        {
            return 0;
        }

        return lagHits.Count;
    }
}
