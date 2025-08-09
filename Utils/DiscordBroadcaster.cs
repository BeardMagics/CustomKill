using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using System;
using System.IO;
using CustomKill.Config;
using CustomKill.Database;
using System.Linq;
using VampireCommandFramework;

namespace CustomKill.Utils
{
    public static class DiscordBroadcaster
    {
        private static readonly HttpClient client = new HttpClient();

        public static async void SendKillMessage(string message)
        {
            var webhookUrl = KillfeedSettings.WebhookURL?.Value;

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                Plugin.Logger.LogWarning("[CustomKill] Webhook URL is null or empty. Skipping Discord message.");
                return;
            }

            await PostToWebhook(webhookUrl, message, "[CustomKill] Kill Message");
        }

        public static async Task PostTopStatsToDiscord(ChatCommandContext ctx)
        {
            try
            {
                var stats = PvPStatsService.GetAllStats() ?? new Dictionary<string, PlayerStats>();

                var topPlayers = stats.Values
                    .Where(s => (s?.Kills ?? 0) > 0)
                    .OrderByDescending(s => s.Kills ?? 0)
                    .Take(10)
                    .ToList();

                var clans = PvPStatsService.GetAllClans() ?? new Dictionary<string, List<string>>();
                var clanStats = clans
                    .Select(clan =>
                    {
                        var members = clan.Value ?? new List<string>();
                        var combinedStats = members
                            .Select(name => stats.TryGetValue(name, out var s) ? s : null)
                            .Where(s => s != null)
                            .ToList();

                        return new
                        {
                            ClanName = clan.Key,
                            TotalKills = combinedStats.Sum(s => s.Kills ?? 0),
                            TotalDeaths = combinedStats.Sum(s => s.Deaths ?? 0),
                            TotalAssists = combinedStats.Sum(s => s.Assists ?? 0),
                            TotalDamage = combinedStats.Sum(s => s.Damage ?? 0)
                        };
                    })
                    .OrderByDescending(c => c.TotalKills)
                    .Take(5)
                    .ToList();

                var sb = new StringBuilder();
                sb.AppendLine("**📊 Top 10 Players**");
                for (int i = 0; i < topPlayers.Count; i++)
                {
                    var p = topPlayers[i];
                    sb.AppendLine($"{i + 1}. **{p.Name}** — 🔥 {p.Damage} / 🗡️ {p.Kills} / 💀 {p.Deaths} / 🤝 {p.Assists}");
                }

                sb.AppendLine();
                sb.AppendLine("**🏰 Top 5 Clans**");
                for (int i = 0; i < clanStats.Count; i++)
                {
                    var c = clanStats[i];
                    sb.AppendLine($"{i + 1}. **{c.ClanName}** — 🔥 {c.TotalDamage} / 🗡️ {c.TotalKills} / 💀 {c.TotalDeaths} / 🤝 {c.TotalAssists}");
                }

                var webhookUrl = KillfeedSettings.StatsWebhookURL?.Value;

                if (string.IsNullOrWhiteSpace(webhookUrl))
                {
                    Plugin.Logger.LogWarning("[CustomKill] Stats webhook URL is null or empty. Skipping post.");
                    ctx.Reply("⚠️ Stats webhook URL not configured.");
                    return;
                }

                await PostToWebhook(webhookUrl, sb.ToString(), "[CustomKill] Stats Post");
                ctx.Reply("[ OK ] Stats have been posted to Discord.");
            }
            catch (Exception ex)
            {
                // Log to console
                Plugin.Logger.LogError($"[FAIL] Discord broadcast crash: {ex}");

                // Write detailed error log with inner exceptions
                string folderPath = Path.Combine(Paths.BepInExRootPath, "Logs");
                Directory.CreateDirectory(folderPath);
                string logFile = Path.Combine(folderPath, "customkill_log.log");

                File.AppendAllText(logFile,
                    $"[{DateTime.Now}] Discord broadcast crash:\n" +
                    $"Message: {ex.Message}\n" +
                    $"Stack Trace:\n{ex.StackTrace}\n" +
                    $"{(ex.InnerException != null ? $"Inner Exception: {ex.InnerException}\nInner Stack:\n{ex.InnerException.StackTrace}\n" : "")}\n");

                ctx.Reply("[ FAIL ] Internal error posting stats.");
                throw; // Rethrow for .ptd outer catch logging too
            }
        }

        private static async Task PostToWebhook(string webhookUrl, string message, string logContext)
        {
            var payload = new { content = message };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(webhookUrl, content);
                Plugin.Logger.LogInfo($"{logContext} webhook response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Logger.LogWarning($"{logContext} failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Plugin.Logger.LogError($"{logContext} webhook error: {ex.Message}");
            }
        }
    }
}
