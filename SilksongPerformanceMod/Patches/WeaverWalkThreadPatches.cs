using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class WeaverWalkThreadPatches
{
    private static readonly AccessTools.FieldRef<WeaverWalkThread, float> VisibleRangeRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, float>("visibleRange");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, float> FalloffRangeRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, float>("falloffRange");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, float> FullAlphaRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, float>("fullAlpha");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, SpriteRenderer> SpriteRendererRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, SpriteRenderer>("spriteRenderer");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, float> OffsetXRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, float>("offsetX");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, float> OffsetYRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, float>("offsetY");

    private static readonly AccessTools.FieldRef<WeaverWalkThread, bool> VisibleRef =
        AccessTools.FieldRefAccess<WeaverWalkThread, bool>("visible");

    [HarmonyPatch(typeof(WeaverWalkThread), "Update")]
    [HarmonyPrefix]
    private static bool UpdateWithoutUnnecessaryDistanceSqrt(WeaverWalkThread __instance)
    {
        if (!ModSettings.EnableWeaverWalkThreadFastPath.Value)
        {
            return true;
        }

        Transform weaver = __instance.weaver;
        SpriteRenderer spriteRenderer = SpriteRendererRef(__instance);
        if (weaver == null || spriteRenderer == null)
        {
            return true;
        }

        float falloffRange = FalloffRangeRef(__instance);
        float visibleRange = VisibleRangeRef(__instance);
        if (falloffRange <= 0f || falloffRange <= visibleRange)
        {
            return true;
        }

        Vector3 position = __instance.transform.position;
        Vector2 targetPosition = new(position.x + OffsetXRef(__instance), position.y + OffsetYRef(__instance));
        float sqrDistance = ((Vector2)weaver.position - targetPosition).sqrMagnitude;

        float falloffRangeSqr = falloffRange * falloffRange;
        if (sqrDistance > falloffRangeSqr)
        {
            if (spriteRenderer.enabled)
            {
                spriteRenderer.enabled = false;
            }

            if (VisibleRef(__instance))
            {
                VisibleRef(__instance) = false;
            }

            return false;
        }

        float visibleRangeSqr = visibleRange * visibleRange;
        if (sqrDistance > visibleRangeSqr)
        {
            if (!spriteRenderer.enabled)
            {
                spriteRenderer.enabled = true;
            }

            if (VisibleRef(__instance))
            {
                VisibleRef(__instance) = false;
            }

            float distance = Mathf.Sqrt(sqrDistance);
            float alpha = FullAlphaRef(__instance) - (distance - visibleRange) / (falloffRange - visibleRange);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            return false;
        }

        if (!VisibleRef(__instance))
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(1f, 1f, 1f, FullAlphaRef(__instance));
            VisibleRef(__instance) = true;
        }

        return false;
    }
}
