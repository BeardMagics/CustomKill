using LiteDB;
using System.IO;
using BepInEx;

namespace CustomKill.Database
{
    public sealed class DatabaseWrapper
    {
        private static DatabaseWrapper _instance;

        public static DatabaseWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseWrapper();
                }
                return _instance;
            }
        }

        private LiteDatabase _database;

        public ILiteCollection<PlayerStats> Collection { get; private set; }

        private DatabaseWrapper()
        {
            var configDir = Paths.ConfigPath;
            var dbFolder = Path.Combine(configDir, "CustomKillDB");

            // Ensure the directory exists
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            var databasePath = Path.Combine(dbFolder, "PlayerStats.db");

            Plugin.Logger.LogInfo($"[CustomKill Database path: {databasePath}");

            _database = new LiteDatabase($"Filename={databasePath}; Connection=shared");

            Collection = _database.GetCollection<PlayerStats>("player_stats");
            Collection.EnsureIndex(x => x.Name);
            Collection.EnsureIndex(x => x.SteamID);


        }

        public void Dispose()
        {
            _database?.Dispose();
        }
    }
}
