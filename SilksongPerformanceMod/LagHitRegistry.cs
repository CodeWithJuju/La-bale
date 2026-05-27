using System.Collections.Generic;
using UnityEngine;

namespace SilksongPerformanceMod;

internal static class LagHitRegistry
{
    private static readonly Dictionary<int, HashSet<HealthManager>> ManagersBySource = new();

    private static readonly Dictionary<int, HashSet<int>> SourceIdsByManager = new();

    public static void Register(GameObject source, HealthManager healthManager)
    {
        if (source == null || healthManager == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!ManagersBySource.TryGetValue(sourceId, out HashSet<HealthManager>? healthManagers))
        {
            healthManagers = new HashSet<HealthManager>();
            ManagersBySource[sourceId] = healthManagers;
        }

        healthManagers.Add(healthManager);

        int healthManagerId = healthManager.GetInstanceID();
        if (!SourceIdsByManager.TryGetValue(healthManagerId, out HashSet<int>? sourceIds))
        {
            sourceIds = new HashSet<int>();
            SourceIdsByManager[healthManagerId] = sourceIds;
        }

        sourceIds.Add(sourceId);
    }

    public static bool TryCopySnapshot(GameObject source, List<HealthManager> snapshot)
    {
        snapshot.Clear();
        if (source == null)
        {
            return false;
        }

        if (!ManagersBySource.TryGetValue(source.GetInstanceID(), out HashSet<HealthManager>? healthManagers))
        {
            return false;
        }

        foreach (HealthManager healthManager in healthManagers)
        {
            if (healthManager != null)
            {
                snapshot.Add(healthManager);
            }
        }

        return snapshot.Count > 0;
    }

    public static void Unregister(GameObject source, HealthManager healthManager)
    {
        if (source == null || healthManager == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        int healthManagerId = healthManager.GetInstanceID();

        if (ManagersBySource.TryGetValue(sourceId, out HashSet<HealthManager>? healthManagers))
        {
            healthManagers.Remove(healthManager);
            if (healthManagers.Count == 0)
            {
                ManagersBySource.Remove(sourceId);
            }
        }

        if (SourceIdsByManager.TryGetValue(healthManagerId, out HashSet<int>? sourceIds))
        {
            sourceIds.Remove(sourceId);
            if (sourceIds.Count == 0)
            {
                SourceIdsByManager.Remove(healthManagerId);
            }
        }
    }

    public static void UnregisterAll(HealthManager healthManager)
    {
        if (healthManager == null)
        {
            return;
        }

        int healthManagerId = healthManager.GetInstanceID();
        if (!SourceIdsByManager.TryGetValue(healthManagerId, out HashSet<int>? sourceIds))
        {
            return;
        }

        foreach (int sourceId in sourceIds)
        {
            if (ManagersBySource.TryGetValue(sourceId, out HashSet<HealthManager>? healthManagers))
            {
                healthManagers.Remove(healthManager);
                if (healthManagers.Count == 0)
                {
                    ManagersBySource.Remove(sourceId);
                }
            }
        }

        SourceIdsByManager.Remove(healthManagerId);
    }

    public static void PruneSource(GameObject source)
    {
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!ManagersBySource.TryGetValue(sourceId, out HashSet<HealthManager>? healthManagers))
        {
            return;
        }

        healthManagers.RemoveWhere(static healthManager => healthManager == null);
        if (healthManagers.Count == 0)
        {
            ManagersBySource.Remove(sourceId);
        }
    }

    public static void UnregisterSource(GameObject source)
    {
        if (source == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        if (!ManagersBySource.TryGetValue(sourceId, out HashSet<HealthManager>? healthManagers))
        {
            return;
        }

        foreach (HealthManager healthManager in healthManagers)
        {
            if (healthManager == null)
            {
                continue;
            }

            int healthManagerId = healthManager.GetInstanceID();
            if (SourceIdsByManager.TryGetValue(healthManagerId, out HashSet<int>? sourceIds))
            {
                sourceIds.Remove(sourceId);
                if (sourceIds.Count == 0)
                {
                    SourceIdsByManager.Remove(healthManagerId);
                }
            }
        }

        ManagersBySource.Remove(sourceId);
    }

    public static void PruneAllStaleEntries()
    {
        List<int>? emptySources = null;
        foreach (KeyValuePair<int, HashSet<HealthManager>> pair in ManagersBySource)
        {
            pair.Value.RemoveWhere(static healthManager => healthManager == null);
            if (pair.Value.Count == 0)
            {
                emptySources ??= new List<int>();
                emptySources.Add(pair.Key);
            }
        }

        if (emptySources != null)
        {
            for (int i = 0; i < emptySources.Count; i++)
            {
                ManagersBySource.Remove(emptySources[i]);
            }
        }

        List<int>? emptyManagers = null;
        foreach (KeyValuePair<int, HashSet<int>> pair in SourceIdsByManager)
        {
            if (pair.Value.Count == 0)
            {
                emptyManagers ??= new List<int>();
                emptyManagers.Add(pair.Key);
            }
        }

        if (emptyManagers != null)
        {
            for (int i = 0; i < emptyManagers.Count; i++)
            {
                SourceIdsByManager.Remove(emptyManagers[i]);
            }
        }
    }
}
