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
            sceneLoad.IsGarbageCollectRequired = true;
            RuntimeContext.Log.LogInfo("Queued unused asset cleanup for this scene transition.");
        }
    }
}
