using System.Linq;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;
using Unity.Collections;
using System.Text;
using System.Collections.Generic;
using CustomKill.Database;
using CustomKill.Config;

namespace CustomKill.Commands
{
    internal class Top
    {
        public static class TopCommand
        {
            [Command("top", "top5", usage: "[kills|deaths|assists|ms]", description: "Displays the top 5 players by category.", adminOnly: false)]
            public static void HandleCommand(ChatCommandContext ctx, string category = null)
            {
                Plugin.Logger.LogInfo($"[TopCommand] Called by {ctx.Event.User.CharacterName} with category: {category}");

                var allStats = PvPStatsService.GetAllStats();
                if (allStats.Count == 0)
                {
                    ctx.Reply("<color=#aaaaaa>No player stats found.</color>");
                    return;
                }

                // Build usage response with dynamic "(Admin Only)" tags
                string usageHelp = "<color=#ff5555>Missing or invalid category.</color> Usage: <color=#ffff00>.top [category]</color>\n" +
                                   "Categories:\n" +
                                   $" <color=#ffaa00>damage</color>{(KillfeedSettings.RestrictDamageToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>kills</color>{(KillfeedSettings.RestrictKillsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>deaths</color>{(KillfeedSettings.RestrictDeathsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>assists</color>{(KillfeedSettings.RestrictAssistsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>ms</color>{(KillfeedSettings.RestrictMaxStreakToAdmin.Value ? " (Admin Only)" : "")}";

                if (string.IsNullOrWhiteSpace(category))
                {
                    ctx.Reply(usageHelp);
                    return;
                }

                string normalizedCategory = category.ToLower();
                if (normalizedCategory == "ms") normalizedCategory = "maxstreak";

                // Admin restrictions based on config
                if (normalizedCategory == "damage" && KillfeedSettings.RestrictDamageToAdmin.Value && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply("<color=#ff5555>Access to the 'damage' leaderboard is restricted to admins only.</color>");
                    return;
                }

                if (normalizedCategory == "kills" && KillfeedSettings.RestrictKillsToAdmin.Value && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply("<color=#ff5555>Access to the 'kills' leaderboard is restricted to admins only.</color>");
                    return;
                }

                if (normalizedCategory == "deaths" && KillfeedSettings.RestrictDeathsToAdmin.Value && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply("<color=#ff5555>Access to the 'deaths' leaderboard is restricted to admins only.</color>");
                    return;
                }

                if (normalizedCategory == "assists" && KillfeedSettings.RestrictAssistsToAdmin.Value && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply("<color=#ff5555>Access to the 'assists' leaderboard is restricted to admins only.</color>");
                    return;
                }

                if (normalizedCategory == "maxstreak" && KillfeedSettings.RestrictMaxStreakToAdmin.Value && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply("<color=#ff5555>Access to the 'maxstreak' leaderboard is restricted to admins only.</color>");
                    return;
                }

                var validCategories = new[] { "damage", "kills", "deaths", "assists", "maxstreak" };
                if (!validCategories.Contains(normalizedCategory))
                {
                    ctx.Reply(usageHelp);
                    return;
                }

                var topPlayers = normalizedCategory switch
                {
                    "damage" => allStats.Where(p => (p.Value.Damage ?? 0) > 0).OrderByDescending(p => p.Value.Damage ?? 0).Take(5),
                    "kills" => allStats.Where(p => (p.Value.Kills ?? 0) > 0).OrderByDescending(p => p.Value.Kills ?? 0).Take(5),
                    "deaths" => allStats.Where(p => (p.Value.Deaths ?? 0) > 0).OrderByDescending(p => p.Value.Deaths ?? 0).Take(5),
                    "assists" => allStats.Where(p => (p.Value.Assists ?? 0) > 0).OrderByDescending(p => p.Value.Assists ?? 0).Take(5),
                    "maxstreak" => allStats.Where(p => (p.Value.MaxStreak ?? 0) > 0).OrderByDescending(p => p.Value.MaxStreak ?? 0).Take(5),
                    _ => null
                };

                if (topPlayers == null || !topPlayers.Any())
                {
                    ctx.Reply($"<color=#aaaaaa>No {normalizedCategory} to display.</color>");
                    return;
                }

                var message = $"<color={ColorSettings.Top_TitleColor.Value}>Top 5 players by {normalizedCategory}:</color>\n";
                int rank = 1;

                foreach (var player in topPlayers)
                {
                    string color = normalizedCategory switch
                    {
                        "damage" => ColorSettings.Top_DamageColor.Value,
                        "kills" => ColorSettings.Top_KillsColor.Value,
                        "deaths" => ColorSettings.Top_DeathsColor.Value,
                        "assists" => ColorSettings.Top_AssistsColor.Value,
                        "maxstreak" => ColorSettings.Top_MaxStreakColor.Value,
                        _ => "#ffffff"
                    };

                    int value = normalizedCategory switch
                    {
                        "damage" => player.Value.Damage ?? 0,
                        "kills" => player.Value.Kills ?? 0,
                        "deaths" => player.Value.Deaths ?? 0,
                        "assists" => player.Value.Assists ?? 0,
                        "maxstreak" => player.Value.MaxStreak ?? 0,
                        _ => 0
                    };

                    message += $"{rank}. <color={ColorSettings.Top_PlayerNameColor.Value}>{player.Key}</color> - <color={color}>{value}</color>\n";
                    rank++;
                }

                ctx.Reply(message);
            }
        }
    }
}
