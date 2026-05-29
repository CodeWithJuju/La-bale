using System;
using System.Collections.Generic;
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

    // CORRIGIDO: substituído ConditionalWeakTable<WaterPhysics, SpreadBuffers> por Dictionary simples.
    //
    // Motivo: ConditionalWeakTable usa lock interno em cada chamada a GetOrCreateValue para
    // garantir thread-safety. Em Unity, todo código de gameplay roda na main thread, portanto
    // esse lock era overhead puro: acontecia 50+ vezes por segundo para cada instância de
    // WaterPhysics presente na cena, sem nenhum benefício.
    //
    // Dictionary<int, SpreadBuffers> keyed por GetInstanceID() tem lookup O(1) sem lock,
    // e a entrada é removida no OnDestroy do WaterPhysics para não acumular entradas mortas.
    private static readonly Dictionary<int, SpreadBuffers> Buffers = new();

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

        int instanceId = __instance.GetInstanceID();
        if (!Buffers.TryGetValue(instanceId, out SpreadBuffers? buffers))
        {
            buffers = new SpreadBuffers();
            Buffers[instanceId] = buffers;
        }

        buffers.EnsureLength(positions.Length);

        // Passo 1: força de mola + atualização de posição e velocidade para cada ponto.
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

        // Passo 2: 8 iterações de propagação lateral (spread) — modifica APENAS velocidades.
        //
        // CORRIGIDO: removido o loop de posição que existia após este bloco na versão anterior.
        //
        // Motivo do bug original: o loop de posição estava FORA do for(j < 8), então:
        //   1. buffers.Left[k] e buffers.Right[k] eram sobrescritos a cada iteração de j,
        //      portanto após o loop continham apenas os valores da 8ª iteração (não a soma).
        //   2. As posições eram modificadas UMA vez com esses valores obsoletos, quando o
        //      original não modifica posições no spread phase — apenas velocidades.
        //   3. Resultado: deslocamento extra incorreto aplicado às posições em cada FixedUpdate,
        //      fazendo a água oscilar de forma errada e causando trabalho extra desnecessário.
        //
        // O spread phase do WaterPhysics modifica somente velocidades; posições são atualizadas
        // no início do próximo FixedUpdate via "positions[i].y += velocities[i]" (Passo 1 acima).
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

        return false;
    }

    // CORRIGIDO: limpa a entrada do dicionário quando WaterPhysics é destruído,
    // evitando que entradas mortas se acumulem ao longo da sessão.
    [HarmonyPatch(typeof(WaterPhysics), "OnDestroy")]
    [HarmonyPostfix]
    private static void CleanupBuffersOnDestroy(WaterPhysics __instance)
    {
        if (ModSettings.EnableWaterPhysicsAllocationFix.Value)
        {
            Buffers.Remove(__instance.GetInstanceID());
        }
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
