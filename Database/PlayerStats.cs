using System;
using LiteDB;

namespace CustomKill.Database
{
    public class PlayerStats
    {
        [BsonId] // Name is the primary key
        public string Name { get; set; }

        public ulong SteamID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Initialize all nullable ints to 0 so they never stay null
        public int? Kills { get; set; } = 0;
        public int? Damage { get; set; } = 0;
        public int? DamageTaken { get; set; } = 0;
        public int? Deaths { get; set; } = 0;
        public int? Assists { get; set; } = 0;
        public int? MaxStreak { get; set; } = 0;
        public int? KillStreak { get; set; } = 0;

        public int? MaxGearScore { get; set; } = 0;
    }
}
