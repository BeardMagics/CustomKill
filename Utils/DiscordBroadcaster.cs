using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;

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
                Plugin.Logger.LogWarning("[BestKillfeed] Webhook URL is null or empty. Skipping Discord message.");
                return;
            }

            var payload = new { content = message };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(webhookUrl, content);
                Plugin.Logger.LogInfo($"[BestKillfeed] Discord webhook response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Logger.LogWarning($"[BestKillfeed] Failed to send webhook: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Plugin.Logger.LogError($"[BestKillfeed] Webhook error: {ex.Message}");
            }
        }
    }
}
