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
        [Command("exportstats", description: "Exports all player stats and clan members to a CSV file.", adminOnly: true)]
        public static void ExportStats(ChatCommandContext ctx)
        {
            try
            {
                // Get all data
                var players = PvPStatsService.GetAllStats();             // PlayerStats Dictionary
                var clans = PvPStatsService.GetAllClans();               // Clan Dictionary

                var sb = new StringBuilder();

                // Reverse mapping: player name -> clan name
                var playerToClan = new Dictionary<string, string>();
                foreach (var kvp in clans)
                {
                    foreach (var member in kvp.Value)
                    {
                        if (!playerToClan.ContainsKey(member))
                            playerToClan[member] = kvp.Key;
                    }
                }

                // CSV Header
                sb.AppendLine("PlayerName,ClanName,ClanTag,Damage,Kills,Deaths,Assists");

                // Player rows
                foreach (var kvp in players.OrderBy(p => p.Value.Name))
                {
                    var player = kvp.Value;
                    var clanName = playerToClan.TryGetValue(player.Name, out var cn) ? cn : "";
                    var clanTag = !string.IsNullOrWhiteSpace(clanName) ? TruncateClan(clanName) : "---";

                    sb.AppendLine($"{player.Name},{clanName},{clanTag},{player.Damage ?? 0},{player.Kills ?? 0},{player.Deaths ?? 0},{player.Assists ?? 0}");
                }

                // Save to CSV file
                var folderPath = Path.Combine(BepInEx.Paths.PluginPath, "CustomKill", "Exports");
                Directory.CreateDirectory(folderPath);

                var fileName = $"PvPStatsExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var fullPath = Path.Combine(folderPath, fileName);

                File.WriteAllText(fullPath, sb.ToString());

                ctx.Reply($"Export completed. File saved to: {fileName}");
                Plugin.Logger.LogInfo($"[ExportStatsCommand] Exported stats to {fullPath}");
            }
            catch (Exception ex)
            {
                ctx.Reply($"Export failed: {ex.Message}");
                Plugin.Logger.LogError($"[ExportStatsCommand] Error: {ex}");
            }
        }

        private static string TruncateClan(string name)
        {
            if (string.IsNullOrEmpty(name)) return "---";
            return name.Length <= 3
                ? name.ToUpper()
                : name.Substring(0, 3).ToUpper();
        }
    }
}
