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
    private sealed class CachedPostprocessModules
    {
        public readonly List<IPostprocessModule> Modules = new();

        public int LastRefreshFrame = int.MinValue;
    }

    private static readonly AccessTools.FieldRef<DamageEnemies, int> DamageEnemiesStepsToNextHitRef =
        AccessTools.FieldRefAccess<DamageEnemies, int>("stepsToNextHit");

    private static readonly AccessTools.FieldRef<DamageEnemies, HashSet<Collider2D>> DamageEnemiesFrameQueueRef =
        AccessTools.FieldRefAccess<DamageEnemies, HashSet<Collider2D>>("frameQueue");

    private static readonly FieldInfo HealthManagerRunningLagHitsField =
        AccessTools.Field(typeof(HealthManager), "runningLagHits");

    private static readonly AccessTools.FieldRef<UberPostprocess, Material> UberPostprocessMaterialRef =
        AccessTools.FieldRefAccess<UberPostprocess, Material>("material");

    private static readonly Dictionary<int, CachedPostprocessModules> UberModuleCache = new();

    private static readonly List<HealthManager> LagHitSnapshot = new();

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

        LagHitRegistry.PruneAllStaleEntries();

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
    }

    [HarmonyPatch(typeof(UberPostprocess), "OnDisable")]
    [HarmonyPostfix]
    private static void ClearUberPostprocessCache(UberPostprocess __instance)
    {
        UberModuleCache.Remove(__instance.GetInstanceID());
    }

    [HarmonyPatch(typeof(UberPostprocess), "OnRenderImage")]
    [HarmonyPrefix]
    [HarmonyPriority(ModHarmonyPriority.PostProcessLast)]
    private static bool OptimizeUberPostprocess(
        UberPostprocess __instance,
        RenderTexture source,
        RenderTexture destination)
    {
        if (!ModSettings.EnableUberPostprocessModuleCache.Value)
        {
            return true;
        }

        Material material = UberPostprocessMaterialRef(__instance);
        if (material == null)
        {
            return true;
        }

        List<IPostprocessModule> modules = GetCachedUberModules(__instance);
        for (int i = 0; i < modules.Count; i++)
        {
            IPostprocessModule module = modules[i];
            if (module is not MonoBehaviour behaviour || behaviour == null)
            {
                continue;
            }

            if (behaviour.enabled)
            {
                material.EnableKeyword(module.EffectKeyword);
                module.UpdateProperties(material);
            }
            else
            {
                material.DisableKeyword(module.EffectKeyword);
            }
        }

        Graphics.Blit(source, destination, material, 0);
        if ((bool)GameCameraTextureDisplay.Instance)
        {
            GameCameraTextureDisplay.Instance.UpdateDisplay(source, material);
        }

        return false;
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

    private static List<IPostprocessModule> GetCachedUberModules(UberPostprocess uberPostprocess)
    {
        int instanceId = uberPostprocess.GetInstanceID();
        if (!UberModuleCache.TryGetValue(instanceId, out CachedPostprocessModules? cachedModules))
        {
            cachedModules = new CachedPostprocessModules();
            UberModuleCache[instanceId] = cachedModules;
        }

        bool needsRefresh = cachedModules.Modules.Count == 0 ||
            Time.frameCount - cachedModules.LastRefreshFrame >= ModSettings.UberPostprocessRefreshInterval.Value;

        if (!needsRefresh)
        {
            for (int i = 0; i < cachedModules.Modules.Count; i++)
            {
                if (cachedModules.Modules[i] is not MonoBehaviour behaviour || behaviour == null)
                {
                    needsRefresh = true;
                    break;
                }
            }
        }

        if (!needsRefresh)
        {
            return cachedModules.Modules;
        }

        cachedModules.Modules.Clear();
        MonoBehaviour[] behaviours = uberPostprocess.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPostprocessModule module)
            {
                cachedModules.Modules.Add(module);
            }
        }

        cachedModules.LastRefreshFrame = Time.frameCount;
        return cachedModules.Modules;
    }
}
