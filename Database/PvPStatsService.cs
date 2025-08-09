using BepInEx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace CustomKill.Database
{
    public static class PvPStatsService
    {
        private static readonly Dictionary<string, PlayerStats> _pending = new();
        private static readonly object _lock = new();
        private static bool _running = false;

        public static void Init()
        {
            if (!_running)
            {
                _running = true;
                Task.Run(() => FlushLoop());
                Plugin.Logger.LogInfo("[PvPStatsService] Buffer initialized.");
            }
        }

        public static Dictionary<string, PlayerStats> GetAllStats()
        {
            return DatabaseWrapper.Instance.Collection.FindAll().ToDictionary(ps => ps.Name, ps => ps);
        }

        public static Dictionary<string, List<string>> GetAllClans()
        {
            var db = DatabaseWrapper.Instance;
            var allMembers = db.ClanMembersCollection.FindAll();

            // Safely build SteamID -> Name map
            var playerStats = new Dictionary<ulong, string>();
            foreach (var p in db.Collection.FindAll())
            {
                if (!playerStats.ContainsKey(p.SteamID))
                    playerStats.Add(p.SteamID, p.Name);
            }

            var clans = new Dictionary<string, List<string>>();
            foreach (var m in allMembers)
            {
                if (!clans.ContainsKey(m.ClanName))
                    clans[m.ClanName] = new List<string>();

                if (playerStats.TryGetValue(m.SteamID, out var name))
                    clans[m.ClanName].Add(name);
                else
                    clans[m.ClanName].Add($"Unknown({m.SteamID})");
            }

            return clans;
        }

        public static PlayerStats GetStats(string name)
        {
            return DatabaseWrapper.Instance.Collection.FindById(name) ?? new PlayerStats { Name = name };
        }

        public static void BufferPvPStat(string name, ulong steamID, bool isKill, bool isDeath, bool isAssist)
        {
            lock (_lock)
            {
                if (!_pending.TryGetValue(name, out var stats))
                {
                    stats = new PlayerStats
                    {
                        Name = name,
                        SteamID = steamID,
                        Kills = 0,
                        Deaths = 0,
                        Assists = 0,
                        MaxStreak = 0,
                        KillStreak = 0
                    };
                    _pending[name] = stats;
                }

                if (isKill)
                {
                    stats.Kills++;
                    stats.KillStreak++;
                    if (stats.KillStreak > stats.MaxStreak) stats.MaxStreak = stats.KillStreak;
                }

                if (isDeath)
                {
                    stats.Deaths++;
                    stats.KillStreak = 0;
                }

                if (isAssist)
                {
                    stats.Assists++;
                }
            }
        }

        private static async Task FlushLoop()
        {
            while (_running)
            {
                Flush();
                await Task.Delay(5000);
            }
        }

        private static void Flush()
        {
            Dictionary<string, PlayerStats> copy;
            lock (_lock)
            {
                if (_pending.Count == 0) return;
                copy = new(_pending);
                _pending.Clear();
            }

            var collection = DatabaseWrapper.Instance.Collection;

            foreach (var pair in copy)
            {
                var stats = pair.Value;
                var existing = collection.FindById(stats.Name);

                if (existing == null)
                {
                    collection.Insert(stats);
                }
                else
                {
                    existing.Kills = (existing.Kills ?? 0) + stats.Kills;
                    existing.Deaths = (existing.Deaths ?? 0) + stats.Deaths;
                    existing.Assists = (existing.Assists ?? 0) + stats.Assists;
                    existing.KillStreak = stats.KillStreak;
                    if (existing.MaxStreak < stats.MaxStreak) existing.MaxStreak = stats.MaxStreak;

                    collection.Update(existing);
                }
            }
        }

        public static void RegisterDamage(string name, ulong steamID, int amount)
        {
            var collection = DatabaseWrapper.Instance.Collection;

            var stats = collection.FindById(name)
                         ?? new PlayerStats { Name = name, SteamID = steamID };

            stats.Damage = (stats.Damage ?? 0) + amount;

            collection.Upsert(stats);
        }

        public static void RegisterDamageTaken(string name, ulong steamID, int amount)
        {
            var collection = DatabaseWrapper.Instance.Collection;

            var stats = collection.FindById(name)
                         ?? new PlayerStats { Name = name, SteamID = steamID };

            stats.DamageTaken = (stats.DamageTaken ?? 0) + amount;

            collection.Upsert(stats);
        }
    }
}
