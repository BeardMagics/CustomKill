using LiteDB;
using System;
using System.IO;
using BepInEx;

namespace CustomKill.Database
{
    /// <summary>
    /// Represents a record of a player’s membership in a clan.
    /// </summary>
    public class ClanMemberRecord
    {
        // Composite Id: "{SteamID}:{ClanName}" to ensure uniqueness per membership
        public string Id { get; set; }
        public ulong SteamID { get; set; }
        public string ClanName { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public sealed class DatabaseWrapper : IDisposable
    {
        private static DatabaseWrapper _instance;
        public static DatabaseWrapper Instance => _instance ??= new DatabaseWrapper();

        private readonly LiteDatabase _database;

        // Core player-stats collection
        public ILiteCollection<PlayerStats> Collection { get; }

        // New clan-members collection
        public ILiteCollection<ClanMemberRecord> ClanMembersCollection
            => _database.GetCollection<ClanMemberRecord>("clan_members");

        private DatabaseWrapper()
        {
            var configDir = Paths.ConfigPath;
            var dbFolder = Path.Combine(configDir, "CustomKillDB");
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            var databasePath = Path.Combine(dbFolder, "PlayerStats.db");
            Plugin.Logger.LogInfo($"[CustomKill] Database path: {databasePath}");

            _database = new LiteDatabase($"Filename={databasePath}; Connection=shared");

            Collection = _database.GetCollection<PlayerStats>("player_stats");
            Collection.EnsureIndex(x => x.Name);
            Collection.EnsureIndex(x => x.SteamID);

            // Ensure index on clan-members for fast lookups
            ClanMembersCollection.EnsureIndex(x => x.Id);
            ClanMembersCollection.EnsureIndex(x => x.JoinedAt);
        }

        public void Dispose()
        {
            _database?.Dispose();
        }
    }
}
