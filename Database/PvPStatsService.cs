using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomKill.Database
{
    public static class PvPStatsService
    {
        public static void RegisterPvPStats(string name, ulong steamID, bool isKill, bool isDeath, bool isAssist)
        {
            try
            {
                var collection = DatabaseWrapper.Instance.Collection;

                var stats = collection.FindById(name);
                if (stats == null ||
                    (stats.Kills == null &&
                     stats.Deaths == null &&
                     stats.Assists == null &&
                     stats.MaxStreak == null &&
                     stats.KillStreak == null))
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

                    collection.Insert(stats);
                }

                Plugin.Logger.LogInfo($"Registering PvP stats for {name} - Kill: {isKill}, Death: {isDeath}, Assist: {isAssist}");

                if (isKill)
                {
                    stats.Kills = (stats.Kills ?? 0) + 1;
                    stats.KillStreak = (stats.KillStreak ?? 0) + 1;

                    if (stats.KillStreak > stats.MaxStreak)
                        stats.MaxStreak = stats.KillStreak;
                }

                if (isDeath)
                {
                    stats.Deaths = (stats.Deaths ?? 0) + 1;
                    stats.KillStreak = 0;
                }

                if (isAssist)
                {
                    stats.Assists = (stats.Assists ?? 0) + 1;
                }

                collection.Update(stats);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[PvPStatsService] Error while registering: {ex.Message}");
            }
        }

        public static void RegisterDamage(string name, ulong steamID, int amount)
        {
            var collection = DatabaseWrapper.Instance.Collection;

            // Find existing or create new with a proper SteamID
            var stats = collection.FindById(name)
                     ?? new PlayerStats { Name = name, SteamID = steamID };

            // Coalesce null to 0, then add
            stats.Damage = (stats.Damage ?? 0) + amount;

            // Upsert will insert if new, or update if existing
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

        public static Dictionary<string, PlayerStats> GetAllStats()
        {
            var collection = DatabaseWrapper.Instance.Collection;
            return collection.FindAll().ToDictionary(ps => ps.Name, ps => ps);
        }

        public static PlayerStats GetStats(string name)
        {
            var collection = DatabaseWrapper.Instance.Collection;
            // Note: this returns a fresh PlayerStats if none existed, but does not persist it.
            return collection.FindById(name)
                   ?? new PlayerStats { Name = name };
        }

        public static void Init()
        {
            _ = DatabaseWrapper.Instance;
            Plugin.Logger.LogInfo("[PvPStatsService] Database initialized.");
        }
    }
}
