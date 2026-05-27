using System.Collections.Generic;
using UnityEngine;

namespace SilksongPerformanceMod;

internal static class PlanarReflectionRegistry
{
    private static readonly HashSet<PlanarRealtimeReflection> Active = new();

    public static void Register(PlanarRealtimeReflection reflection)
    {
        if (reflection != null)
        {
            Active.Add(reflection);
        }
    }

    public static void Unregister(PlanarRealtimeReflection reflection)
    {
        if (reflection != null)
        {
            Active.Remove(reflection);
        }
    }

    public static void ApplyTuningToAll()
    {
        if (!RuntimeContext.IsAggressiveGpuModeEnabled())
        {
            return;
        }

        foreach (PlanarRealtimeReflection reflection in Active)
        {
            if (reflection != null)
            {
                RuntimeContext.OptimizePlanarReflection(reflection);
            }
        }
    }

    public static void PruneDestroyed()
    {
        Active.RemoveWhere(static reflection => reflection == null);
    }

    public static void Clear()
    {
        Active.Clear();
    }
}
