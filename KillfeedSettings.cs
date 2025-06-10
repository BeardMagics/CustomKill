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

        // Discord webhook
        public static ConfigEntry<string> WebhookURL;

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

        public static void Init(string configPath)
        {
            var config = new ConfigFile(Paths.ConfigPath + "\\CustomKill.cfg", true);

            // Discord
            WebhookURL = config.Bind("Discord", "WebhookURL", "", "Discord Webhook URL for broadcasting kill messages.");

            // Admin-only restrictions for .top command
            RestrictDamageToAdmin = config.Bind("TopAccess", "RestrictDamageToAdmin", false, "Restrict access to the 'damage' leaderboard to admins only.");
            RestrictKillsToAdmin = config.Bind("TopAccess", "RestrictKillsToAdmin", false, "Restrict access to the 'kills' leaderboard to admins only.");
            RestrictDeathsToAdmin = config.Bind("TopAccess", "RestrictDeathsToAdmin", true, "Restrict access to the 'deaths' leaderboard to admins only.");
            RestrictAssistsToAdmin = config.Bind("TopAccess", "RestrictAssistsToAdmin", false, "Restrict access to the 'assists' leaderboard to admins only.");
            RestrictMaxStreakToAdmin = config.Bind("TopAccess", "RestrictMaxStreakToAdmin", false, "Restrict access to the 'maxstreak' leaderboard to admins only.");

            // Colors
            KillerNameColor = config.Bind("Colors", "KillerNameColor", "#ffffff", "Color of the killer's name.");
            VictimNameColor = config.Bind("Colors", "VictimNameColor", "#ffffff", "Color of the victim's name.");
            ClanTagColor = config.Bind("Colors", "ClanTagColor", "#888888", "Color of the clan tags.");
            AllowedLevelColor = config.Bind("Colors", "AllowedLevelColor", "#55ff55", "Color for allowed kills (fair level difference).");
            ForbiddenLevelColor = config.Bind("Colors", "ForbiddenLevelColor", "#ff5555", "Color for forbidden kills (too high level difference).");

            // Kill message format
            KillMessageFormat = config.Bind("Message", "KillMessageFormat",
                "<color={ClanTagColor}>[{KillerClan}]</color><color={KillerNameColor}>{Killer}</color>[<color={LevelColor}>{KillerLevel}</color>] killed <color={ClanTagColor}>[{VictimClan}]</color><color={VictimNameColor}>{Victim}</color>[<color={LevelColor}>{VictimLevel}</color>]",
                "Killfeed message format. Available placeholders: {Killer}, {Victim}, {KillerClan}, {VictimClan}, {KillerLevel}, {VictimLevel}, {LevelColor}, {KillerNameColor}, {VictimNameColor}, {ClanTagColor}");

            // Level restrictions
            MaxLevelGapNormal = config.Bind("Restrictions", "MaxLevelGapNormal", 15, "Maximum level difference allowed for fair kills when killer is below level 91.");
            MaxLevelGapHigh = config.Bind("Restrictions", "MaxLevelGapHigh", 21, "Maximum level difference allowed for fair kills when killer is level 91 or higher.");

            Plugin.Logger.LogInfo($"[CustomKill] Loaded WebhookURL from config: {WebhookURL.Value}");
        }
    }
}
