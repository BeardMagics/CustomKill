using BepInEx.Configuration;
using System.IO;

namespace CustomKill.Config
{
    public static class ColorSettings
    {
        // .top Config Entries
        public static ConfigEntry<string> Top_TitleColor;
        public static ConfigEntry<string> Top_DamageColor;
        public static ConfigEntry<string> Top_KillsColor;
        public static ConfigEntry<string> Top_DeathsColor;
        public static ConfigEntry<string> Top_AssistsColor;
        public static ConfigEntry<string> Top_MaxStreakColor;
        public static ConfigEntry<string> Top_PlayerNameColor;

        // .stats Config Entries
        public static ConfigEntry<string> Stats_TitleColor;
        public static ConfigEntry<string> Stats_DamageColor;
        public static ConfigEntry<string> Stats_KillsColor;
        public static ConfigEntry<string> Stats_DeathsColor;
        public static ConfigEntry<string> Stats_AssistsColor;
        public static ConfigEntry<string> Stats_KillStreakColor;
        public static ConfigEntry<string> Stats_MaxStreakColor;
        public static ConfigEntry<string> Stats_PlayerNameColor;

        public static void Init(string configPath)
        {
            // TOP CONFIG
            var topPath = Path.Combine(configPath, "CustomKill_top.cfg");
            var topConfig = new ConfigFile(topPath, true);

            Top_TitleColor = topConfig.Bind("TopColors", "TitleColor", "#ffaa00", "Color for the title in .top");
            Top_DamageColor = topConfig.Bind("TopColors", "DamageColor", "#ffffff", "Color for the damage in .top");
            Top_KillsColor = topConfig.Bind("TopColors", "KillsColor", "#55ff55", "Color for kills shown");
            Top_DeathsColor = topConfig.Bind("TopColors", "DeathsColor", "#ff5555", "Color for deaths shown");
            Top_AssistsColor = topConfig.Bind("TopColors", "AssistsColor", "#aaaaaa", "Color for assists shown");
            Top_MaxStreakColor = topConfig.Bind("TopColors", "MaxStreakColor", "#55aaff", "Color for max streak shown");
            Top_PlayerNameColor = topConfig.Bind("TopColors", "PlayerNameColor", "#ffffff", "Color for player names");

            // STATS CONFIG
            var statsPath = Path.Combine(configPath, "CustomKill_stats.cfg");
            var statsConfig = new ConfigFile(statsPath, true);

            Stats_TitleColor = statsConfig.Bind("StatsColors", "TitleColor", "#00aaff", "Color for the title in .stats");
            Stats_DamageColor = statsConfig.Bind("StatsColors", "DamageColor", "#ffffff", "Color for the damage in .stats");
            Stats_KillsColor = statsConfig.Bind("StatsColors", "KillsColor", "#55ff55", "Color for kills shown");
            Stats_DeathsColor = statsConfig.Bind("StatsColors", "DeathsColor", "#ff5555", "Color for deaths shown");
            Stats_AssistsColor = statsConfig.Bind("StatsColors", "AssistsColor", "#aaaaaa", "Color for assists shown");
            Stats_KillStreakColor = statsConfig.Bind("StatsColors", "KillStreakColor", "#ffff55", "Color for current kill streak");
            Stats_MaxStreakColor = statsConfig.Bind("StatsColors", "MaxStreakColor", "#55aaff", "Color for max streak");
            Stats_PlayerNameColor = statsConfig.Bind("StatsColors", "PlayerNameColor", "#ffffff", "Color for player names");
        }
    }
}
