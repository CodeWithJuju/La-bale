using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

[HarmonyPatch]
internal static class WaterPhysicsPatches
{
    private static readonly AccessTools.FieldRef<WaterPhysics, Vector2[]> PositionsRef =
        AccessTools.FieldRefAccess<WaterPhysics, Vector2[]>("positions");

    private static readonly AccessTools.FieldRef<WaterPhysics, float[]> VelocitiesRef =
        AccessTools.FieldRefAccess<WaterPhysics, float[]>("velocities");

    private static readonly AccessTools.FieldRef<WaterPhysics, float[]> AccelerationsRef =
        AccessTools.FieldRefAccess<WaterPhysics, float[]>("accelerations");

    private static readonly AccessTools.FieldRef<WaterPhysics, LineRenderer> LineRendererRef =
        AccessTools.FieldRefAccess<WaterPhysics, LineRenderer>("lineRenderer");

    private static readonly AccessTools.FieldRef<WaterPhysics, float> TopRef =
        AccessTools.FieldRefAccess<WaterPhysics, float>("top");

    private static readonly ConditionalWeakTable<WaterPhysics, SpreadBuffers> Buffers = new();

    [HarmonyPatch(typeof(WaterPhysics), "FixedUpdate")]
    [HarmonyPrefix]
    private static bool FixedUpdateWithoutPerTickArrays(WaterPhysics __instance)
    {
        if (!ModSettings.EnableWaterPhysicsAllocationFix.Value)
        {
            return true;
        }

        Vector2[] positions = PositionsRef(__instance);
        float[] velocities = VelocitiesRef(__instance);
        float[] accelerations = AccelerationsRef(__instance);
        LineRenderer lineRenderer = LineRendererRef(__instance);
        if (positions == null || velocities == null || accelerations == null || lineRenderer == null || positions.Length == 0)
        {
            return true;
        }

        float top = TopRef(__instance);
        SpreadBuffers buffers = Buffers.GetOrCreateValue(__instance);
        buffers.EnsureLength(positions.Length);

        for (int i = 0; i < positions.Length; i++)
        {
            float acceleration = 0f - (__instance.spring * (positions[i].y - top) + velocities[i] * __instance.damping);
            accelerations[i] = acceleration;

            Vector2 position = positions[i];
            position.y += velocities[i];
            positions[i] = position;
            velocities[i] += acceleration;

            lineRenderer.SetPosition(i, new Vector3(position.x, position.y, __instance.transform.position.z + __instance.lineZ));
        }

        for (int j = 0; j < 8; j++)
        {
            for (int k = 0; k < positions.Length; k++)
            {
                if (k > 0)
                {
                    buffers.Left[k] = __instance.spread * (positions[k].y - positions[k - 1].y);
                    velocities[k - 1] += buffers.Left[k];
                }

                if (k < positions.Length - 1)
                {
                    buffers.Right[k] = __instance.spread * (positions[k].y - positions[k + 1].y);
                    velocities[k + 1] += buffers.Right[k];
                }
            }
        }

        for (int l = 0; l < positions.Length; l++)
        {
            if (l > 0)
            {
                Vector2 leftPosition = positions[l - 1];
                leftPosition.y += buffers.Left[l];
                positions[l - 1] = leftPosition;
            }

            if (l < positions.Length - 1)
            {
                Vector2 rightPosition = positions[l + 1];
                rightPosition.y += buffers.Right[l];
                positions[l + 1] = rightPosition;
            }
        }

        return false;
    }

    private sealed class SpreadBuffers
    {
        public float[] Left { get; private set; } = Array.Empty<float>();

        public float[] Right { get; private set; } = Array.Empty<float>();

        public void EnsureLength(int length)
        {
            if (Left.Length == length)
            {
                return;
            }

            Left = new float[length];
            Right = new float[length];
        }
    }
}
