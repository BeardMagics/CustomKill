using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace CustomKill.Database
{
    public class PlayerStats
    {
        [BsonId] // Will mark name field as primary key
        public string Name { get; set; }
        public ulong SteamID { get; set; } // Optional, can be null if not available
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Automatically set to current time when created
        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public int? Assists { get; set; }
        public int? MaxStreak { get; set; }
        public int? KillStreak { get; set; } // Current kill streak, starts at 0

    }
}
