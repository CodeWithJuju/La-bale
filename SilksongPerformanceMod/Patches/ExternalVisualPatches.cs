using System.Collections.Generic;
using Ara;
using HarmonyLib;
using TeamCherry.NestedFadeGroup;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class ExternalVisualPatches
{
    private static readonly HashSet<int> QueuedTk2dTextMeshIds = new();

    private static readonly Plane[] AraTrailFrustumPlanes = new Plane[6];

    // Cache do frustum por frame: evita recalcular para cada trail na mesma frame.
    // CalculateFrustumPlanes é uma operação de matriz não trivial — chamá-la
    // uma vez por frame em vez de uma vez por trail elimina o custo redundante.
    private static int _frustumCacheFrame = -1;
    private static Camera? _frustumCacheCamera;

    [HarmonyPatch(typeof(tk2dUpdateManager), "QueueCommitInternal")]
    [HarmonyPrefix]
    private static bool DeduplicateTk2dQueue(tk2dTextMesh textMesh)
    {
        if (!ModSettings.EnableTk2dQueueDedup.Value || textMesh == null)
        {
            return true;
        }

        return QueuedTk2dTextMeshIds.Add(textMesh.GetInstanceID());
    }

    [HarmonyPatch(typeof(tk2dUpdateManager), "FlushQueuesInternal")]
    [HarmonyPostfix]
    private static void ClearTk2dQueueCache()
    {
        QueuedTk2dTextMeshIds.Clear();
    }

    [HarmonyPatch]
    internal static class ExtendedExternalVisualPatches
    {
        internal static bool Prepare()
        {
            return ModHarmonyPrepare.ShouldPatchExtendedCpuPatches();
        }

        [HarmonyPatch(typeof(NestedFadeGroupMixer), "LateUpdate")]
        [HarmonyPrefix]
        private static bool ThrottleNestedFadeGroupMixer(NestedFadeGroupMixer __instance)
        {
            if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() ||
                !ModSettings.EnableNestedFadeMixerThrottle.Value ||
                !RuntimeContext.ShouldApplyDistanceThrottle())
            {
                return true;
            }

            return RuntimeContext.ShouldRunForDistance(
                __instance.transform,
                ModSettings.NestedFadeMixerNearHeroDistance.Value,
                ModSettings.NestedFadeMixerNearCameraDistance.Value,
                ModSettings.NestedFadeMixerInterval.Value);
        }

        [HarmonyPatch(typeof(AraTrail), "UpdateTrailMesh")]
        [HarmonyPrefix]
        private static bool CullOffscreenAraTrail(AraTrail __instance, Camera cam)
        {
            if (!Application.isPlaying || !RuntimeContext.IsGameplayCriticalState())
            {
                return true;
            }

            if (!RuntimeContext.IsPerFrameHarmonyThrottleEnabled() ||
                !ModSettings.EnableAraTrailOffscreenCulling.Value ||
                cam == null)
            {
                return true;
            }

            Transform trailTransform = __instance.transform;
            Vector3 cameraPosition = cam.transform.position;
            float maxDistance = ModSettings.AraTrailOffscreenDistance.Value;
            if ((cameraPosition - trailTransform.position).sqrMagnitude <= maxDistance * maxDistance)
            {
                return true;
            }

            Mesh mesh = __instance.mesh;
            if (mesh == null || mesh.vertexCount <= 1)
            {
                return true;
            }

            Bounds localBounds = mesh.bounds;
            if (localBounds.extents.sqrMagnitude <= Mathf.Epsilon)
            {
                return true;
            }

            int currentFrame = Time.frameCount;
            if (_frustumCacheFrame != currentFrame || _frustumCacheCamera != cam)
            {
                GeometryUtility.CalculateFrustumPlanes(cam, AraTrailFrustumPlanes);
                _frustumCacheFrame = currentFrame;
                _frustumCacheCamera = cam;
            }

            Matrix4x4 worldMatrix = (__instance.space == Space.Self && trailTransform.parent != null)
                ? trailTransform.parent.localToWorldMatrix
                : Matrix4x4.identity;

            Bounds worldBounds = TransformBounds(localBounds, worldMatrix);

            if (GeometryUtility.TestPlanesAABB(AraTrailFrustumPlanes, worldBounds))
            {
                return true;
            }

            return false;
        }

        private static Bounds TransformBounds(Bounds localBounds, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));

            Vector3 worldExtents = new(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));

            return new Bounds(center, worldExtents * 2f);
        }
    }
}