using CustomKill.Database;
using CustomKill.Services;
using System;
using System.Linq;
using VampireCommandFramework;

namespace CustomKill.Commands
{
    internal class LeaderboardCommand
    {
        [Command("leaderboard", "lb", description: "Displays the player leaderboard by kills", adminOnly: false)]
        public static void HandleCommand(ChatCommandContext ctx, string arg = null)
        {
            int pageSize = 10;
            int page = 1;

            if (!string.IsNullOrWhiteSpace(arg) && int.TryParse(arg, out var parsedPage))
            {
                page = parsedPage;
            }

            var allStats = PvPStatsService.GetAllStats();
            var sortedStats = allStats.OrderByDescending(kv => kv.Value.Kills).ToList();
            var totalPages = (int)Math.Ceiling((double)sortedStats.Count / pageSize);

            page = Math.Clamp(page, 1, totalPages);

            var header = $"<color=#ffaa00>--- Leaderboard (Page {page}/{totalPages}) ---</color>";
            ctx.Reply(header);

            int startIndex = (page - 1) * pageSize;

            for (int i = startIndex; i < Math.Min(startIndex + pageSize, sortedStats.Count); i++)
            {
                var (name, stats) = (sortedStats[i].Key, sortedStats[i].Value);
                int rank = i + 1;

                var line = $"<color=#aaaaaa>#{rank}</color> <color=#ffffff>{name}</color> - " +
                           $"<color=#ffffff>#{KillfeedSettings.RestrictDamageToAdmin.Value}{stats.Damage}</color> " +
                           $"Kills: <color=#55ff55>{KillfeedSettings.RestrictKillsToAdmin.Value}{stats.Kills}</color> / " +
                           $"Deaths: <color=#ff5555>{KillfeedSettings.RestrictDeathsToAdmin.Value}{stats.Deaths}</color> / " +
                           $"Assists: <color=#ff555{KillfeedSettings.RestrictAssistsToAdmin.Value}{stats.Assists}</color> / " +
                           $"Max Streak: <color=#55aaff>{KillfeedSettings.RestrictMaxStreakToAdmin.Value}{stats.MaxStreak}</color>";
                ctx.Reply(line);
            }

            // Always show caller's own stats
            var playerName = ctx.Event.User.CharacterName.ToString();
            var playerStats = PvPStatsService.GetStats(playerName);
            var playerRank = sortedStats.FindIndex(kv => kv.Key == playerName) + 1;

            if (playerRank > 0)
            {
                var selfLine = $"<color=#ffaa00>#{playerRank}</color> <color=#ffffff>{playerName}</color> - " +
                               $"<color=#ffffff>#{playerStats.Damage}</color> " +
                               $"Kills: <color=#55ff55>{playerStats.Kills}</color> / " +
                               $"Deaths: <color=#ff5555>{playerStats.Deaths}</color> / " +
                               $"Assists: <color=#ff5555>{playerStats.Assists}</color> / " +
                               $"Current Streak: <color=#ff5555>{playerStats.KillStreak}</color> / " +
                               $"Max Streak: <color=#55aaff>{playerStats.MaxStreak}</color>";
                ctx.Reply(selfLine);
            }
        }
    }
}