using System.Collections.Generic;
using UnityEngine;

namespace SilksongPerformanceMod;

internal static class LagHitRegistry
{
    private static readonly Dictionary<int, HashSet<HealthManager>> ManagersBySource = new();

    private static readonly Dictionary<int, HashSet<int>> SourceIdsByManager = new();

    private static readonly Dictionary<int, GameObject> SourcesById = new();

    private static readonly List<int> EmptySourcesScratch = new();

    private static readonly List<int> EmptyManagersScratch = new();

    public static void Register(GameObject source, HealthManager healthManager)
    {
        if (source == null || healthManager == null)
        {
            return;
        }

        int sourceId = source.GetInstanceID();
        SourcesById[sourceId] = source;

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
                SourcesById.Remove(sourceId);
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
                    SourcesById.Remove(sourceId);
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
            SourcesById.Remove(sourceId);
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
        SourcesById.Remove(sourceId);
    }

    public static void PruneAllStaleEntries()
    {
        EmptySourcesScratch.Clear();
        EmptyManagersScratch.Clear();

        foreach (KeyValuePair<int, HashSet<HealthManager>> pair in ManagersBySource)
        {
            if (!SourcesById.TryGetValue(pair.Key, out GameObject? source) || source == null)
            {
                RemoveSourceIdFromManagers(pair.Key, pair.Value);
                EmptySourcesScratch.Add(pair.Key);
                continue;
            }

            pair.Value.RemoveWhere(static healthManager => healthManager == null);
            if (pair.Value.Count == 0)
            {
                EmptySourcesScratch.Add(pair.Key);
            }
        }

        for (int i = 0; i < EmptySourcesScratch.Count; i++)
        {
            int sourceId = EmptySourcesScratch[i];
            ManagersBySource.Remove(sourceId);
            SourcesById.Remove(sourceId);
        }

        foreach (KeyValuePair<int, HashSet<int>> pair in SourceIdsByManager)
        {
            if (pair.Value.Count == 0)
            {
                EmptyManagersScratch.Add(pair.Key);
            }
        }

        for (int i = 0; i < EmptyManagersScratch.Count; i++)
        {
            SourceIdsByManager.Remove(EmptyManagersScratch[i]);
        }
    }

    private static void RemoveSourceIdFromManagers(int sourceId, HashSet<HealthManager> healthManagers)
    {
        foreach (HealthManager healthManager in healthManagers)
        {
            if (healthManager == null)
            {
                continue;
            }

            int healthManagerId = healthManager.GetInstanceID();
            if (!SourceIdsByManager.TryGetValue(healthManagerId, out HashSet<int>? sourceIds))
            {
                continue;
            }

            sourceIds.Remove(sourceId);
            if (sourceIds.Count == 0)
            {
                EmptyManagersScratch.Add(healthManagerId);
            }
        }
    }
}
