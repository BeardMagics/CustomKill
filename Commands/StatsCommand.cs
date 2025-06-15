using CustomKill.Database;
using ProjectM;
using ProjectM.Network;
using System.Linq;
using Unity.Entities;
using VampireCommandFramework;
using CustomKill.Config;

namespace CustomKill.Commands
{
    public static class StatsCommand
    {
        [Command("stats", usage: "playerName", description: "Shows your PvP stats or another player's.", adminOnly: false)]
        public static void HandleCommand(ChatCommandContext ctx, string playerName = null)
        {
            string requester = ctx.Event.User.CharacterName.ToString();
            bool isSelf = string.IsNullOrWhiteSpace(playerName) || playerName == requester;
            string targetName = isSelf ? requester : playerName;

            var stats = PvPStatsService.GetStats(targetName);

            bool hasNoStats = (stats.Kills ?? 0) == 0 &&
                              (stats.Deaths ?? 0) == 0 &&
                              (stats.Assists ?? 0) == 0 &&
                              (stats.KillStreak ?? 0) == 0 &&
                              (stats.MaxStreak ?? 0) == 0 &&
                              stats.Damage.GetValueOrDefault() == 0;

            if (hasNoStats)
            {
                ctx.Reply(isSelf
                    ? "<color=#aaaaaa>You have no PvP stats to display.</color>"
                    : $"<color=#aaaaaa>No stats found for player \"{ColorSettings.Stats_PlayerNameColor} {targetName}\".</color>");
                return;
            }

            bool isAdmin = ctx.Event.User.IsAdmin;

            // Control display based on permissions
            string damageDisplay = (isSelf || isAdmin || !KillfeedSettings.RestrictDamageToAdmin.Value)
                                      ? $"{stats.Damage}" : "<color=#eb34db>Denied</color>";
            string killsDisplay = (isSelf || isAdmin || !KillfeedSettings.RestrictKillsToAdmin.Value)
                                      ? $"{stats.Kills}" : "<color=#eb34db>Denied</color>";
            string deathsDisplay = (isSelf || isAdmin || !KillfeedSettings.RestrictDeathsToAdmin.Value)
                                      ? $"{stats.Deaths}" : "<color=#eb34db>Denied</color>";
            string assistsDisplay = (isSelf || isAdmin || !KillfeedSettings.RestrictAssistsToAdmin.Value)
                                      ? $"{stats.Assists}" : "<color=#eb34db>Denied</color>";
            string maxStreakDisplay = (isSelf || isAdmin || !KillfeedSettings.RestrictMaxStreakToAdmin.Value)
                                      ? $"{stats.MaxStreak}" : "<color=#eb34db>Denied</color>";
            string killStreakDisplay = $"{stats.KillStreak ?? 0}";

            // Build the output message, now including damage
            string message =
                $"<color={ColorSettings.Stats_TitleColor.Value}>Displaying stats for</color> <color={ColorSettings.Stats_PlayerNameColor.Value}>{targetName}</color>\n" +
                $"Damage: <color={ColorSettings.Stats_DamageColor.Value}>{damageDisplay}</color> | " +
                $"Kills: <color={ColorSettings.Stats_KillsColor.Value}>{killsDisplay}</color> | " +
                $"Deaths: <color={ColorSettings.Stats_DeathsColor.Value}>{deathsDisplay}</color> | " +
                $"Assists: <color={ColorSettings.Stats_AssistsColor.Value}>{assistsDisplay}</color>\n" +
                $"Kill Streak: <color={ColorSettings.Stats_KillStreakColor.Value}>{killStreakDisplay}</color> | " +
                $"Max Streak: <color={ColorSettings.Stats_MaxStreakColor.Value}>{maxStreakDisplay}</color>";

            ctx.Reply(message);
        }
    }
}
