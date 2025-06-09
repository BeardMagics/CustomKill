using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using CustomKill;

namespace CustomKill.Utils
{
    public static class KillCache
    {
        private const float AssistDecaySeconds = 12f;

        private static readonly Dictionary<Entity, List<(Entity Attacker, DateTime Time)>> AssistMap = new();
        private static readonly Dictionary<Entity, List<(Entity Attacker, DateTime Time)>> LockedAssists = new();
        private static readonly Dictionary<Entity, Entity> DownerMap = new();

        public static void AddAssist(Entity victim, Entity attacker)
        {
            var now = DateTime.UtcNow;

            if (!AssistMap.TryGetValue(victim, out var assists))
            {
                assists = new List<(Entity, DateTime)>();
                AssistMap[victim] = assists;
            }

            // Always refresh the timestamp on hit
            var index = assists.FindIndex(a => a.Attacker == attacker);
            if (index >= 0)
            {
                assists[index] = (attacker, now); // Refresh timestamp
            }
            else
            {
                assists.Add((attacker, now)); // New assist
            }
        }

        public static List<Entity> GetAssists(Entity victim, Entity killer = default)
        {
            if (!AssistMap.TryGetValue(victim, out var assists)) return new List<Entity>();

            var now = DateTime.UtcNow;
            return assists
                .Where(entry => (now - entry.Time).TotalSeconds <= AssistDecaySeconds && entry.Attacker != killer)
                .Select(entry => entry.Attacker)
                .ToList();
        }

        public static void LockAssists(Entity victim)
        {
            if (AssistMap.TryGetValue(victim, out var assists))
            {
                var now = DateTime.UtcNow;
                var validAssists = assists
                    .Where(entry => (now - entry.Time).TotalSeconds <= AssistDecaySeconds)
                    .ToList();

                if (validAssists.Count > 0)
                {
                    LockedAssists[victim] = validAssists;
                    Plugin.Logger.LogInfo($"[KillCache] Locked {validAssists.Count} valid assists for victim {victim.Index}");
                }
            }
        }

        public static List<Entity> GetLockedAssists(Entity victim, Entity killer = default)
        {
            if (!LockedAssists.TryGetValue(victim, out var assists)) return new List<Entity>();

            return assists
                .Where(entry => entry.Attacker != killer)
                .Select(entry => entry.Attacker)
                .ToList();
        }

        public static bool HasLockedAssists(Entity victim)
        {
            return LockedAssists.ContainsKey(victim);
        }

        public static void ClearAssists(Entity victim)
        {
            AssistMap.Remove(victim);
            LockedAssists.Remove(victim);
            DownerMap.Remove(victim);
        }

        // Downer tracking
        public static void SetDowner(Entity victim, Entity downer)
        {
            DownerMap[victim] = downer;
        }

        public static Entity GetDowner(Entity victim)
        {
            return DownerMap.TryGetValue(victim, out var downer) ? downer : Entity.Null;
        }

        // Compatibility helpers
        public static bool HasAssist(Entity victim)
        {
            return AssistMap.ContainsKey(victim) && AssistMap[victim].Count > 0;
        }

        public static List<Entity> GetAssistants(Entity victim)
        {
            return GetAssists(victim);
        }

        public static void Clear(Entity victim)
        {
            ClearAssists(victim);
        }
    }
}
