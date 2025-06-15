using VampireCommandFramework;
using CustomKill.Database;

namespace CustomKill.Commands
{
    public static class KDCommand
    {
        [Command("kd", description: "Shows your Kill/Death ratio.", adminOnly: false)]
        public static void ShowKD(ChatCommandContext ctx)
        {
            string characterName = ctx.Event.User.CharacterName.ToString();
            var stats = PvPStatsService.GetStats(characterName);

            int kills = stats.Kills ?? 0;
            int deaths = stats.Deaths ?? 0;

            if (kills == 0 && deaths == 0)
            {
                ctx.Reply("<color=#aaaaaa>You have no kills or deaths recorded yet.</color>");
                return;
            }

            float kd = deaths == 0 ? kills : (float)kills / deaths;
            string kdRatio = kd.ToString("0.00");

            string message = $"<color=#e34d4d>Your K/D Ratio:</color> <color=#FFD700>{kdRatio}</color>";

            ctx.Reply(message);
        }

        [Command("kda", description: "Shows your Kill/Death/Assist ratio.", adminOnly: false)]
        public static void ShowKDA(ChatCommandContext ctx)
        {
            string characterName = ctx.Event.User.CharacterName.ToString();
            var stats = PvPStatsService.GetStats(characterName);

            int kills = stats.Kills ?? 0;
            int deaths = stats.Deaths ?? 0;
            int assists = stats.Assists ?? 0;

            if (kills == 0 && deaths == 0 && assists == 0)
            {
                ctx.Reply("<color=#aaaaaa>You have no K/D/A stats recorded yet.</color>");
                return;
            }

            float effectiveKills = kills + (assists * 0.5f);
            float kda = deaths == 0 ? effectiveKills : effectiveKills / deaths;
            string kdaRatio = kda.ToString("0.00");

            string message = $"<color=#e34d4d>Your K/D/A Ratio:</color> <color=#00FFFF>{kdaRatio}</color>";

            ctx.Reply(message);
        }
    }
}
