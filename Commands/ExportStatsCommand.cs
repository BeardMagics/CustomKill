using VampireCommandFramework;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomKill.Database;
using UnityEngine;

namespace CustomKill.Commands
{
    public static class ExportStatsCommand
    {
        [Command("exportstats", description: "Exports all player and clan stats to wipe cycle HTML file", adminOnly: true)]
        public static void ExportStats(ChatCommandContext ctx)
        {
            try
            {
                var allStats = PvPStatsService.GetAllStats();
                var clans = PvPStatsService.GetAllClans();

                // --- Player Stats HTML ---
                var playerBuilder = new StringBuilder();
                playerBuilder.AppendLine("<!DOCTYPE html>");
                playerBuilder.AppendLine("<html><head><meta charset='UTF-8'><title>Wipe Cycle PvP Stats</title></head>");
                playerBuilder.AppendLine("<body style='font-family:sans-serif;background-color:#111;color:#fff;padding:20px;'>");
                playerBuilder.AppendLine("<h1 style='color:#EB621F;'>Wipe Cycle PvP Stats</h1>");
                playerBuilder.AppendLine("<table style='width:100%;border-collapse:collapse;'>");
                playerBuilder.AppendLine("<tr style='color:#EB621F;'>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>PlayerName</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>ClanName</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>ClanTag</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>SteamID</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>KD Ratio</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>Damage</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>Kills</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>Deaths</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>Assists</th>");
                playerBuilder.AppendLine("<th style='border:1px solid #555;padding:8px;'>Highest Killstreak</th>");
                playerBuilder.AppendLine("</tr>");

                var playerToClan = new Dictionary<string, string>();
                foreach (var kvp in clans)
                {
                    foreach (var member in kvp.Value)
                        if (!playerToClan.ContainsKey(member))
                            playerToClan[member] = kvp.Key;
                }

                foreach (var kvp in allStats.OrderByDescending(p => p.Value.Damage ?? 0))
                {
                    var player = kvp.Value;
                    var clanName = playerToClan.TryGetValue(player.Name, out var cn) ? cn : "";
                    var clanTag = !string.IsNullOrWhiteSpace(clanName) ? (clanName.Length <= 4 ? clanName.ToUpper() : clanName.Substring(0, 4).ToUpper()) : "---";
                    var kdRatio = player.Deaths == 0 ? (player.Kills ?? 0) : Math.Round((double)(player.Kills ?? 0) / (player.Deaths ?? 1), 2);

                    playerBuilder.AppendLine("<tr>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.Name}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{clanName}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{clanTag}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.SteamID}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{kdRatio}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.Damage ?? 0}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.Kills ?? 0}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.Deaths ?? 0}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.Assists ?? 0}</td>");
                    playerBuilder.AppendLine($"<td style='border:1px solid #555;padding:8px;'>{player.MaxStreak}</td>");
                    playerBuilder.AppendLine("</tr>");
                }

                playerBuilder.AppendLine("</table></body></html>");

                var folderPath = Path.Combine(BepInEx.Paths.PluginPath, "CustomKill", "Exports");
                Directory.CreateDirectory(folderPath);
                File.WriteAllText(Path.Combine(folderPath, "WipeCycle_PlayerStats.html"), playerBuilder.ToString());

                // Call clan export
                ExportClanStatsToHtml(allStats, clans, folderPath);

                ctx.Reply("[ OK ] Player + Clan HTML exports complete.");
            }
            catch (Exception ex)
            {
                ctx.Reply($"[ FAIL ] Export failed: {ex.Message}");
                Plugin.Logger.LogError($"[ExportStatsCommand] Error: {ex}");
            }
        }

        private static void ExportClanStatsToHtml(Dictionary<string, PlayerStats> allStats, Dictionary<string, List<string>> clans, string folderPath)
        {
            var clanStats = new Dictionary<string, ClanAggregate>();
            foreach (var kvp in clans)
            {
                var clanName = kvp.Key;
                if (!clanStats.ContainsKey(clanName))
                    clanStats[clanName] = new ClanAggregate { Name = clanName };

                foreach (var member in kvp.Value)
                {
                    if (allStats.TryGetValue(member, out var stats))
                    {
                        clanStats[clanName].Kills += stats.Kills ?? 0;
                        clanStats[clanName].Damage += stats.Damage ?? 0;
                        clanStats[clanName].Assists += stats.Assists ?? 0;
                        clanStats[clanName].MaxStreak += stats.MaxStreak ?? 0;
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'><title>Wipe Cycle PvP Stats - Clan Totals</title></head>");
            sb.AppendLine("<body style='font-family:sans-serif;background-color:#111;color:#fff;padding:20px;'>");
            sb.AppendLine("<h1 style='color:#EB621F;'>Wipe Cycle PvP Stats - Clan Totals</h1>");

            AppendSection(sb, "Top 10 Clans by Damage", clanStats.Values.OrderByDescending(c => c.Damage).Take(10), c => c.Damage);
            AppendSection(sb, "Top 10 Clans by Kills", clanStats.Values.OrderByDescending(c => c.Kills).Take(10), c => c.Kills);
            AppendSection(sb, "Top 10 Clans by Assists", clanStats.Values.OrderByDescending(c => c.Assists).Take(10), c => c.Assists);
            AppendSection(sb, "Top 10 Clans by Highest Killstreak", clanStats.Values.OrderByDescending(c => c.MaxStreak).Take(10), c => c.MaxStreak);

            sb.AppendLine("</body></html>");
            File.WriteAllText(Path.Combine(folderPath, "WipeCycle_ClanStats.html"), sb.ToString());
        }

        private static void AppendSection(StringBuilder sb, string title, IEnumerable<ClanAggregate> list, Func<ClanAggregate, int> selector)
        {
            sb.AppendLine($"<h2 style='color:#EB621F;'>{title}</h2>");
            sb.AppendLine("<ol>");
            foreach (var clan in list)
            {
                var tag = clan.Name.Length <= 4 ? clan.Name.ToUpper() : clan.Name.Substring(0, 4).ToUpper();
                sb.AppendLine($"<li>{clan.Name} [{tag}] – {selector(clan):N0}</li>");
            }
            sb.AppendLine("</ol>");
        }

        private class ClanAggregate
        {
            public string Name { get; set; } = "";
            public int Damage { get; set; }
            public int Kills { get; set; }
            public int Assists { get; set; }
            public int MaxStreak { get; set; }
        }
    }
}
