using System;

namespace SilksongPerformanceMod;

internal static class SceneMemoryCoordinator
{
    private static int _transitionsSinceCleanup;

    private static bool _isHooked;

    public static void Hook()
    {
        if (_isHooked)
        {
            return;
        }

        GameManager.SceneTransitionBegan += OnSceneTransitionBegan;
        _isHooked = true;
    }

    public static void Unhook()
    {
        if (!_isHooked)
        {
            return;
        }

        GameManager.SceneTransitionBegan -= OnSceneTransitionBegan;
        _isHooked = false;
    }

    private static void OnSceneTransitionBegan(SceneLoad sceneLoad)
    {
        if (sceneLoad == null)
        {
            return;
        }

        sceneLoad.ActivationComplete += OnActivationComplete;

        void OnActivationComplete()
        {
            sceneLoad.ActivationComplete -= OnActivationComplete;

            if (!ModSettings.EnableSceneTransitionMemoryCleanup.Value)
            {
                return;
            }

            int interval = Math.Max(1, ModSettings.SceneMemoryCleanupInterval.Value);
            _transitionsSinceCleanup++;
            if (_transitionsSinceCleanup < interval)
            {
                return;
            }

            _transitionsSinceCleanup = 0;
            sceneLoad.IsUnloadAssetsRequired = true;

            // CORRIGIDO: o código anterior forçava sempre IsGarbageCollectRequired = true,
            // disparando uma coleta GC bloqueante (compacting) em cada transição elegível.
            //
            // Problema: quando EnableGcSmoothing está desabilitado (padrão), os GcPatches
            // não interceptam essa coleta, então ela roda como blocking GC completo —
            // exatamente o que o mod deveria evitar. Isso causava spikes visíveis a cada
            // SceneMemoryCleanupInterval transições de cena.
            //
            // Solução: quando EnableGcSmoothing está ativo, usar PerformSoftGc que escolhe
            // coleta incremental (se suportada) ou optimized não-bloqueante com cooldown.
            // Apenas sem GcSmoothing mantemos o comportamento de IsGarbageCollectRequired,
            // que já é o comportamento esperado pelo usuário que escolheu não suavizar o GC.
            if (ModSettings.EnableGcSmoothing.Value)
            {
                RuntimeContext.PerformSoftGc("SceneMemoryCoordinator");
            }
            else
            {
                sceneLoad.IsGarbageCollectRequired = true;
            }

            RuntimeContext.Log.LogInfo("Queued unused asset cleanup for this scene transition.");
        }
    }
}
