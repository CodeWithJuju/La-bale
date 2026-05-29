using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

/// <summary>
/// Replaces UberPostprocess.OnRenderImage when enabled. Not injected in High FPS mode
/// because it runs every rendered frame and can cost more than it saves at 120+ FPS.
/// </summary>
[HarmonyPatch]
internal static class UberPostprocessPatches
{
    private sealed class CachedPostprocessModules
    {
        public readonly List<IPostprocessModule> Modules = new();
        public readonly List<MonoBehaviour> ComponentScratch = new();
        public int LastRefreshFrame = int.MinValue;
    }

    private static readonly AccessTools.FieldRef<UberPostprocess, Material> UberPostprocessMaterialRef =
        AccessTools.FieldRefAccess<UberPostprocess, Material>("material");

    private static readonly Dictionary<int, CachedPostprocessModules> UberModuleCache = new();

    internal static bool Prepare()
    {
        return ModHarmonyPrepare.ShouldPatchExtendedCpuPatches() &&
               ModSettings.EnableUberPostprocessModuleCache.Value;
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
        cachedModules.ComponentScratch.Clear();
        uberPostprocess.GetComponents(cachedModules.ComponentScratch);
        for (int i = 0; i < cachedModules.ComponentScratch.Count; i++)
        {
            if (cachedModules.ComponentScratch[i] is IPostprocessModule module)
            {
                cachedModules.Modules.Add(module);
            }
        }

        cachedModules.ComponentScratch.Clear();
        cachedModules.LastRefreshFrame = Time.frameCount;
        return cachedModules.Modules;
    }
}
