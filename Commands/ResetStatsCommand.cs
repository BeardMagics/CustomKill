using System.Collections.Generic;
using VampireCommandFramework;
using CustomKill.Database;

namespace CustomKill.Commands
{
    public static class ResetStatsCommands
    {
        // Track pending reset requests by user SteamID
        private static readonly HashSet<ulong> PendingConfirmations = new HashSet<ulong>();

        [Command("resetstats", "rs", description: "Resets all PvP stats and clan memberships (admin only)", adminOnly: true)]
        public static void ResetStats(ChatCommandContext ctx)
        {
            ulong userId = ctx.User.PlatformId;
            if (!PendingConfirmations.Contains(userId))
            {
                PendingConfirmations.Add(userId);
                ctx.Reply("⚠ Are you sure you want to reset ALL PvP stats and clan membership records? This cannot be undone.\n" +
                          "Please confirm by typing `.y` to proceed or `.n` to cancel.");
            }
            else
            {
                ctx.Reply("⚠ A reset confirmation is already pending. Please type `.y` to confirm or `.n` to cancel.");
            }
        }

        [Command("y", description: "Confirm pending reset (admin only)", adminOnly: true)]
        public static void ConfirmReset(ChatCommandContext ctx)
        {
            ulong userId = ctx.User.PlatformId;
            if (PendingConfirmations.Contains(userId))
            {
                // Delete all player stats
                var statsCollection = DatabaseWrapper.Instance.Collection;
                int statsDeleted = statsCollection.DeleteAll();

                // Delete all clan membership records
                var clanCollection = DatabaseWrapper.Instance.ClanMembersCollection;
                int clansDeleted = clanCollection.DeleteAll();

                ctx.Reply($"[ ! ] Reset complete. Deleted {statsDeleted} player stat entries and {clansDeleted} clan membership entries.");
                PendingConfirmations.Remove(userId);
            }
        }

        [Command("n", description: "Cancel pending reset (admin only)", adminOnly: true)]
        public static void CancelReset(ChatCommandContext ctx)
        {
            ulong userId = ctx.User.PlatformId;
            if (PendingConfirmations.Contains(userId))
            {
                ctx.Reply("[ X ] Reset cancelled. No data was changed.");
                PendingConfirmations.Remove(userId);
            }
        }
    }
}
