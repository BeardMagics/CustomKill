using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;
using CustomKill.Database;
using CustomKill.Config;

namespace CustomKill.Commands
{
    internal class Top
    {
        public static class TopCommand
        {
            [Command("top", usage: "[category] [subcategory]", description: "Displays leaderboards for players or clans.", adminOnly: false)]
            public static void HandleCommand(ChatCommandContext ctx, string category = null, string metric = null)
            {
                // Normalize inputs
                category = (category ?? string.Empty).ToLowerInvariant();
                metric = (metric ?? string.Empty).ToLowerInvariant();

                // === Clan Leaderboard ===
                if (category == "clan")
                {
                    // Validate sub‐metric
                    if (metric != "kills" && metric != "damage")
                    {
                        ctx.Reply($"<color={ColorSettings.Top_ClanTitleColor.Value}>Usage:</color> <color={ColorSettings.Top_ClanTitleColor.Value}>.top clan [category]</color>\n" +
                            $" <color={ColorSettings.Top_ClanTitleColor.Value}>Categories:</color>\n" +
                            $" <color={ColorSettings.Top_ClanKillsColor.Value}>kills</color>\n" +
                            $" <color={ColorSettings.Top_ClanDamageColor.Value}>damage</color>\n");
                        return;
                    }

                    // Only include members older than configured window // default: >1 week old
                    int days = KillfeedSettings.ClanTrackingDays.Value;
                    DateTime cutoff = DateTime.UtcNow.AddDays(-days);
                    var members = DatabaseWrapper.Instance.ClanMembersCollection
                        .Find(x => x.JoinedAt <= cutoff)
                        .ToList();

                    if (!members.Any())
                    {
                        ctx.Reply($"<color={ColorSettings.Top_ClanTitleColor.Value}>No clans with members older than {days} day{(days == 1 ? "" : "s")}.</color>");
                        return;
                    }

                    // Aggregate kills/damage
                    var clanStats = members
                        .GroupBy(m => m.ClanName)
                        .Select(g => new
                        {
                            Clan = g.Key,
                            Kills = g.Sum(m => PvPStatsService.GetStats(m.SteamID.ToString()).Kills ?? 0),
                            Damage = g.Sum(m => PvPStatsService.GetStats(m.SteamID.ToString()).Damage ?? 0)
                        })
                        .ToList();

                    // Pick top 3
                    var topClans = (metric == "damage"
                        ? clanStats.OrderByDescending(c => c.Damage)
                        : clanStats.OrderByDescending(c => c.Kills))
                        .Take(3)
                        .ToList();

                    if (!topClans.Any())
                    {
                        ctx.Reply($"<color=#aaaaaa>No clan stats to display.</color>");
                        return;
                    }

                    // Build the message
                    var sbClan = new StringBuilder();
                    sbClan.AppendLine(metric == "damage"
                        ? $"<color={ColorSettings.Top_ClanTitleColor.Value}>Top Clans by</color> <color={ColorSettings.Top_ClanDamageColor.Value}Damage 🏆</color>"
                        : $"<color={ColorSettings.Top_ClanTitleColor.Value}>Top Clans by</color> <color={ColorSettings.Top_ClanKillsColor.Value}Kills 🏆</color>");
                    for (int i = 0; i < topClans.Count; i++)
                    {
                        var c = topClans[i];
                        int value = metric == "damage" ? c.Damage : c.Kills;
                        string clanValColor = metric == "damage"
                            ? ColorSettings.Top_ClanDamageColor.Value
                            : ColorSettings.Top_ClanKillsColor.Value;
                        sbClan.AppendLine(
                            $"{i + 1}. <color={ColorSettings.Top_ClanNameColor.Value}>{c.Clan}</color> — " +
                            $"<color={clanValColor}>{value}</color>"
                        );
                    }
                    ctx.Reply(sbClan.ToString());
                    return;
                }
                // === Player Leaderboard ===
                var allStats = PvPStatsService.GetAllStats();
                if (allStats.Count == 0)
                {
                    ctx.Reply("<color=#aaaaaa>No player stats found.</color>");
                    return;
                }

                string usageHelp = "<color=#ff5555>Missing or invalid category.</color> Usage: <color=#ffff00>.top [category]</color>\n" +
                                   "Categories:\n" +
                                   $" <color=#ffaa00>clan</color>)\n" +
                                   $" <color=#ffaa00>damage</color>{(KillfeedSettings.RestrictDamageToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>kills</color>{(KillfeedSettings.RestrictKillsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>deaths</color>{(KillfeedSettings.RestrictDeathsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>assists</color>{(KillfeedSettings.RestrictAssistsToAdmin.Value ? " (Admin Only)" : "")}\n" +
                                   $" <color=#ffaa00>maxstreak</color>{(KillfeedSettings.RestrictMaxStreakToAdmin.Value ? " (Admin Only)" : "")}";

                if (string.IsNullOrWhiteSpace(category))
                {
                    ctx.Reply(usageHelp);
                    return;
                }

                if (category == "ms") category = "maxstreak";

                var validCategories = new[] { "clan", "damage", "kills", "deaths", "assists", "maxstreak" };
                if (!validCategories.Contains(category))
                {
                    ctx.Reply(usageHelp);
                    return;
                }

                bool restricted =
                    (category == "damage" && KillfeedSettings.RestrictDamageToAdmin.Value) ||
                    (category == "kills" && KillfeedSettings.RestrictKillsToAdmin.Value) ||
                    (category == "deaths" && KillfeedSettings.RestrictDeathsToAdmin.Value) ||
                    (category == "assists" && KillfeedSettings.RestrictAssistsToAdmin.Value) ||
                    (category == "maxstreak" && KillfeedSettings.RestrictMaxStreakToAdmin.Value);
                if (restricted && !ctx.Event.User.IsAdmin)
                {
                    ctx.Reply($"<color=#ff5555>Access to the '{category}' leaderboard is restricted to admins only.</color>");
                    return;
                }

                var topPlayers = (category switch
                {
                    "damage" => allStats.Where(p => (p.Value.Damage ?? 0) > 0).OrderByDescending(p => p.Value.Damage).Take(5),
                    "kills" => allStats.Where(p => (p.Value.Kills ?? 0) > 0).OrderByDescending(p => p.Value.Kills).Take(5),
                    "deaths" => allStats.Where(p => (p.Value.Deaths ?? 0) > 0).OrderByDescending(p => p.Value.Deaths).Take(5),
                    "assists" => allStats.Where(p => (p.Value.Assists ?? 0) > 0).OrderByDescending(p => p.Value.Assists).Take(5),
                    "maxstreak" => allStats.Where(p => (p.Value.MaxStreak ?? 0) > 0).OrderByDescending(p => p.Value.MaxStreak).Take(5),
                    _ => Enumerable.Empty<KeyValuePair<string, PlayerStats>>()
                }).ToList();

                if (!topPlayers.Any())
                {
                    ctx.Reply($"<color=#aaaaaa>No {category} to display.</color>");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"<color={ColorSettings.Top_TitleColor.Value}>Top 5 players by {category}:</color>");
                int rank = 1;
                foreach (var player in topPlayers)
                {
                    string colValue = category switch
                    {
                        "damage" => ColorSettings.Top_DamageColor.Value,
                        "kills" => ColorSettings.Top_KillsColor.Value,
                        "deaths" => ColorSettings.Top_DeathsColor.Value,
                        "assists" => ColorSettings.Top_AssistsColor.Value,
                        "maxstreak" => ColorSettings.Top_MaxStreakColor.Value,
                        _ => "#ffffff"
                    };
                    int val = category switch
                    {
                        "damage" => player.Value.Damage ?? 0,
                        "kills" => player.Value.Kills ?? 0,
                        "deaths" => player.Value.Deaths ?? 0,
                        "assists" => player.Value.Assists ?? 0,
                        "maxstreak" => player.Value.MaxStreak ?? 0,
                        _ => 0
                    };
                    sb.AppendLine($"{rank}. <color={ColorSettings.Top_PlayerNameColor.Value}>{player.Key}</color> - <color={colValue}>{val}</color>");
                    rank++;
                }
                ctx.Reply(sb.ToString());
            }
        }
    }
}
