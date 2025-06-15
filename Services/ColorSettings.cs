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

        // .top Config Entries (Clan)
        public static ConfigEntry<string> Top_ClanTitleColor;
        public static ConfigEntry<string> Top_ClanNameColor;
        public static ConfigEntry<string> Top_ClanKillsColor;
        public static ConfigEntry<string> Top_ClanDamageColor;

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

            Top_TitleColor = topConfig.Bind("Top 5 Colors", "TitleColor", "#ffaa00", "Color for the title in .top");
            Top_DamageColor = topConfig.Bind("Top 5 Colors", "DamageColor", "#ffffff", "Color for the damage in .top");
            Top_KillsColor = topConfig.Bind("Top 5 Colors", "KillsColor", "#55ff55", "Color for kills shown");
            Top_DeathsColor = topConfig.Bind("Top 5 Colors", "DeathsColor", "#ff5555", "Color for deaths shown");
            Top_AssistsColor = topConfig.Bind("Top 5 Colors", "AssistsColor", "#aaaaaa", "Color for assists shown");
            Top_MaxStreakColor = topConfig.Bind("Top 5 Colors", "MaxStreakColor", "#55aaff", "Color for max streak shown");
            Top_PlayerNameColor = topConfig.Bind("Top 5 Colors", "PlayerNameColor", "#ffffff", "Color for player names");

            // CLAN CONFIG
            Top_ClanTitleColor = topConfig.Bind("Clan Colors", "ClanTitleColor", "#ffaa00", "Color for the clan title in .top");
            Top_ClanNameColor = topConfig.Bind("Clan Colors", "ClanNameColor", "#ffaa00", "Color for the clan name in .top");
            Top_ClanKillsColor = topConfig.Bind("Clan Colors", "ClanKillsColor", "#55ff55", "Color for clan kills shown");
            Top_ClanDamageColor = topConfig.Bind("Clan Colors", "ClanDamageColor", "#ffffff", "Color for clan damage shown");

            // STATS CONFIG
            var statsPath = Path.Combine(configPath, "CustomKill_stats.cfg");
            var statsConfig = new ConfigFile(statsPath, true);

            Stats_TitleColor = statsConfig.Bind("Stats Colors", "TitleColor", "#34abeb", "Color for the title in .stats");
            Stats_DamageColor = statsConfig.Bind("Stats Colors", "DamageColor", "#EB621F", "Color for the damage in .stats");
            Stats_KillsColor = statsConfig.Bind("Stats Colors", "KillsColor", "#69ff55", "Color for kills shown");
            Stats_DeathsColor = statsConfig.Bind("Stats Colors", "DeathsColor", "#e34d4d", "Color for deaths shown");
            Stats_AssistsColor = statsConfig.Bind("Stats Colors", "AssistsColor", "#63f5ff", "Color for assists shown");
            Stats_KillStreakColor = statsConfig.Bind("Stats Colors", "KillStreakColor", "#e84848", "Color for current kill streak");
            Stats_MaxStreakColor = statsConfig.Bind("Stats Colors", "MaxStreakColor", "#55aaff", "Color for max streak");
            Stats_PlayerNameColor = statsConfig.Bind("Stats Colors", "PlayerNameColor", "#9428fa", "Color for player names");
        }
    }
}
