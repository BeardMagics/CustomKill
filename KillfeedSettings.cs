using BepInEx;
using BepInEx.Configuration;

namespace CustomKill
{
    public static class KillfeedSettings
    {
        // .top Admin Restrictions — updated names to match references in .top command
        public static ConfigEntry<bool> RestrictDamageToAdmin;
        public static ConfigEntry<bool> RestrictKillsToAdmin;
        public static ConfigEntry<bool> RestrictDeathsToAdmin;
        public static ConfigEntry<bool> RestrictAssistsToAdmin;
        public static ConfigEntry<bool> RestrictMaxStreakToAdmin;

        // Killfeed Level Tracking mode
        public static ConfigEntry<int> LevelTrackingMode;

        // Days a clan member must be in a clan to be counted in .top clan damage/kills statistics
        internal static ConfigEntry<int> ClanTrackingDays;

        // Discord webhook
        public static ConfigEntry<string> WebhookURL;
        public static ConfigEntry<string> StatsWebhookURL;

        // Color settings
        public static ConfigEntry<string> KillerNameColor;
        public static ConfigEntry<string> VictimNameColor;
        public static ConfigEntry<string> ClanTagColor;
        public static ConfigEntry<string> AllowedLevelColor;
        public static ConfigEntry<string> ForbiddenLevelColor;

        // Message format
        public static ConfigEntry<string> KillMessageFormat;

        // Level restriction settings
        public static ConfigEntry<int> MaxLevelGapNormal;
        public static ConfigEntry<int> MaxLevelGapHigh;

        public static void Init(ConfigFile config)
        {
            LevelTrackingMode = config.Bind(    // Insert level tracking type / select mode setting 1 - max GS - 2 Live ECS snapshot
                "Killfeed",
                "LevelTrackingMode",
                1,
                "1 = Highest equipped gearscore, 2 = Current equipped gearscore on kill"
            );
        }

        public static void Init(string configPath)
        {
            var config = new ConfigFile(Paths.ConfigPath + "\\CustomKill.cfg", true);

            ClanTrackingDays = config.Bind(section: "Clan Member Stats", key: "ClanMemberMinDaysForStats", defaultValue: 7,
             description: "Minimum days a clan member must be in a clan to be counted in .top clan damage/kills statistics.");

            // Discord
            WebhookURL = config.Bind("Discord", "WebhookURL", "", "Discord Webhook URL for broadcasting kill messages.");
            StatsWebhookURL = config.Bind("Discord", "StatsWebhookURL", "", "Discord Webhook URL for broadcasting player stats.");

            // Level Tracking Mode
            LevelTrackingMode = config.Bind("Level Service Mode", "LevelTrackingMode", 1, 
                "Level tracking mode: 1 = Track by max logged gearscore , 2 = Track using live gearscore data (Unity ECS Snapshot at time of kill)");

            // Admin-only restrictions for .top command
            RestrictDamageToAdmin = config.Bind("Top Command Access", "RestrictDamageToAdmin", false, "Restrict access to the 'damage' leaderboard to admins only.");
            RestrictKillsToAdmin = config.Bind("Top Command Access", "RestrictKillsToAdmin", false, "Restrict access to the 'kills' leaderboard to admins only.");
            RestrictDeathsToAdmin = config.Bind("Top Command Access", "RestrictDeathsToAdmin", true, "Restrict access to the 'deaths' leaderboard to admins only.");
            RestrictAssistsToAdmin = config.Bind("Top Command Access", "RestrictAssistsToAdmin", false, "Restrict access to the 'assists' leaderboard to admins only.");
            RestrictMaxStreakToAdmin = config.Bind("Top Command Access", "RestrictMaxStreakToAdmin", false, "Restrict access to the 'maxstreak' leaderboard to admins only.");

            // Colors
            KillerNameColor = config.Bind("Killfeed Colors", "KillerNameColor", "#ffffff", "Color of the killer's name.");
            VictimNameColor = config.Bind("Killfeed Colors", "VictimNameColor", "#ffffff", "Color of the victim's name.");
            ClanTagColor = config.Bind("Killfeed Colors", "ClanTagColor", "#888888", "Color of the clan tags.");
            AllowedLevelColor = config.Bind("Killfeed Colors", "AllowedLevelColor", "#55ff55", "Color for allowed kills (fair level difference).");
            ForbiddenLevelColor = config.Bind("Killfeed Colors", "ForbiddenLevelColor", "#ff5555", "Color for forbidden kills (too high level difference).");

            // Kill message format
            KillMessageFormat = config.Bind("Kill Message Format", "KillMessageFormat",
                "<color={ClanTagColor}>[{KillerClan}]</color><color={KillerNameColor}>{Killer}</color>[<color={LevelColor}>{KillerLevel}</color>] killed <color={ClanTagColor}>[{VictimClan}]</color><color={VictimNameColor}>{Victim}</color>[<color={LevelColor}>{VictimLevel}</color>]",
                "Killfeed message format. Available placeholders: {Killer}, {Victim}, {KillerClan}, {VictimClan}, {KillerLevel}, {VictimLevel}, {LevelColor}, {KillerNameColor}, {VictimNameColor}, {ClanTagColor}");

            // Level restrictions
            MaxLevelGapNormal = config.Bind("Level Restrictions", "MaxLevelGapNormal", 15, "Maximum level difference allowed for fair kills when killer is below level 91.");
            MaxLevelGapHigh = config.Bind("Level Restrictions", "MaxLevelGapHigh", 21, "Maximum level difference allowed for fair kills when killer is level 91 or higher.");

            Plugin.Logger.LogInfo($"[CustomKill] Loaded Discord WebhookURL from config: {WebhookURL.Value}");
        }
    }
}
