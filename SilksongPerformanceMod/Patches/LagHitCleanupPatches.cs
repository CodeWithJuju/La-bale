using HarmonyLib;
using UnityEngine;

namespace SilksongPerformanceMod.Patches;

/// <summary>
/// CORRIGIDO: removido patch em Object.Destroy (interceptava TODA destruição do jogo,
/// incluindo partículas e projéteis em combate intenso — centenas de calls/s).
/// A limpeza agora é lazy: entradas stale são removidas quando consultadas.
/// CancelAllLagHitsForSource já chama PruneSource após usar o snapshot,
/// e UnregisterAll é chamado em OnDisable/CancelAllLagHits — cobertura suficiente.
/// </summary>
[HarmonyPatch]
internal static class LagHitCleanupPatches
{
    // Patch removido intencionalmente.
    // Motivo: HarmonyPatch em UnityEngine.Object.Destroy intercepta toda destruição
    // do jogo (partículas, projéteis, efeitos). Em combate intenso isso gera
    // centenas de prefix calls por segundo, anulando ganhos de outros patches.
    //
    // Alternativa adotada: limpeza lazy via LagHitRegistry.PruneSource e
    // LagHitRegistry.PruneAllStaleEntries, já chamados nos pontos certos em
    // CombatAndPostFxPatches. Entradas com GameObjects destruídos são detectadas
    // pela checagem healthManager == null nos snapshots.
}