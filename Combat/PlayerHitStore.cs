using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomKill.Combat
{
    public struct HitInteraction
    {
        public ulong AttackerSteamId;
        public ulong VictimSteamId;
        public string AttackerName;
        public int AttackerLevel;
        public string VictimName;
        public int VictimLevel;
        public long Timestamp; // Now uses DateTime.UtcNow.Ticks
        public int DmgSourceGUID;
        public int DmgAmount;
    }

    public class PlayerHitData
    {
        public List<HitInteraction> Attacks { get; } = new();
        public List<HitInteraction> Defenses { get; } = new();
    }

    public static class PlayerHitStore
    {
        private const double PVP_WINDOW_SECONDS = 30.0;

        private static readonly Dictionary<ulong, PlayerHitData> interactionsByPlayer =
            new();

        public static IReadOnlyDictionary<ulong, PlayerHitData> InteractionsByPlayer => interactionsByPlayer;

        public static void AddHit(
            ulong attackerSteamId,
            string attackerName,
            int attackerLevel,
            ulong victimSteamId,
            string victimName,
            int victimLevel,
            int dmgSourceGUID,
            int damageAmount)
        {
            var hit = new HitInteraction
            {
                AttackerSteamId = attackerSteamId,
                AttackerName = attackerName,
                AttackerLevel = attackerLevel,
                VictimSteamId = victimSteamId,
                VictimName = victimName,
                VictimLevel = victimLevel,
                Timestamp = DateTime.UtcNow.Ticks,
                DmgSourceGUID = dmgSourceGUID,
                DmgAmount = damageAmount
            };

            AddAttack(attackerSteamId, hit);
            AddDefense(victimSteamId, hit);

            if (interactionsByPlayer.TryGetValue(victimSteamId, out var victimHitData)
                && victimHitData.Defenses.Count >= 500)
            {
                CleanupOldHitInteractionsByPlayer(victimSteamId);
            }
        }

        private static void AddAttack(ulong playerSteamId, HitInteraction hit)
        {
            if (!interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
            {
                hitData = new PlayerHitData();
                interactionsByPlayer[playerSteamId] = hitData;
            }
            hitData.Attacks.Add(hit);
        }

        private static void AddDefense(ulong playerSteamId, HitInteraction hit)
        {
            if (!interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
            {
                hitData = new PlayerHitData();
                interactionsByPlayer[playerSteamId] = hitData;
            }
            hitData.Defenses.Add(hit);
        }

        public static IReadOnlyList<HitInteraction> GetAttacks(ulong playerSteamId)
        {
            if (interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
                return hitData.Attacks;
            return [];
        }

        public static IReadOnlyList<HitInteraction> GetDefenses(ulong playerSteamId)
        {
            if (interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
                return hitData.Defenses;
            return [];
        }

        public static Dictionary<ulong, (string Name, int Level)> GetRecentAttackersWithLvl(
            ulong playerSteamId,
            double pvpWindowSeconds = PVP_WINDOW_SECONDS)
        {
            var result = new Dictionary<ulong, (string, int)>();

            if (!interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
                return result;

            long nowTicks = DateTime.UtcNow.Ticks;
            long windowTicks = TimeSpan.FromSeconds(pvpWindowSeconds).Ticks;

            foreach (HitInteraction hit in hitData.Defenses)
            {
                if (nowTicks - hit.Timestamp > windowTicks) continue;
                if (playerSteamId == hit.AttackerSteamId) continue;

                if (result.TryGetValue(hit.AttackerSteamId, out (string, int) name_lvl))
                {
                    if (hit.AttackerLevel > name_lvl.Item2)
                        result[hit.AttackerSteamId] = (hit.AttackerName, hit.AttackerLevel);
                }
                else
                {
                    result[hit.AttackerSteamId] = (hit.AttackerName, hit.AttackerLevel);
                }
            }

            return result;
        }

        public static int GetHighestLvlUsedOnKiller(
            ulong victimSteamId,
            ulong killerSteamId,
            double pvpWindowSeconds = PVP_WINDOW_SECONDS)
        {
            int peakLevel = -1;

            if (!interactionsByPlayer.TryGetValue(victimSteamId, out var hitData))
                return peakLevel;

            long nowTicks = DateTime.UtcNow.Ticks;
            long windowTicks = TimeSpan.FromSeconds(pvpWindowSeconds).Ticks;

            foreach (HitInteraction hit in hitData.Attacks)
            {
                if (nowTicks - hit.Timestamp > windowTicks) continue;
                if (killerSteamId == hit.VictimSteamId)
                {
                    peakLevel = Math.Max(peakLevel, hit.AttackerLevel);
                }
            }

            return peakLevel;
        }

        public static IReadOnlyList<HitInteraction> GetRecentInteractions(
            ulong playerSteamId,
            double pvpWindowSeconds = PVP_WINDOW_SECONDS)
        {
            if (!interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
                return [];

            long nowTicks = DateTime.UtcNow.Ticks;
            long windowTicks = TimeSpan.FromSeconds(pvpWindowSeconds).Ticks;
            long earliest = nowTicks - windowTicks;

            return hitData.Attacks
                .Concat(hitData.Defenses)
                .Where(hit => hit.Timestamp >= earliest)
                .OrderBy(hit => hit.Timestamp)
                .ToList();
        }

        public static void CleanupOldHitInteractionsByPlayer(
            ulong playerSteamId,
            double pvpWindowSeconds = PVP_WINDOW_SECONDS)
        {
            if (!interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
                return;

            long nowTicks = DateTime.UtcNow.Ticks;
            long windowTicks = TimeSpan.FromSeconds(pvpWindowSeconds).Ticks;

            int before = hitData.Defenses.Count;
            hitData.Defenses.RemoveAll(hit => (nowTicks - hit.Timestamp) > windowTicks);
            int after = hitData.Defenses.Count;

            Plugin.Logger.LogMessage($"CLEANED up {before - after} old hit interactions for SteamID: {playerSteamId}");
        }

        public static void ResetPlayerHitInteractions(ulong playerSteamId)
        {
            if (interactionsByPlayer.TryGetValue(playerSteamId, out var hitData))
            {
                hitData.Attacks.Clear();
                hitData.Defenses.Clear();
            }
        }

        public static void Clear() => interactionsByPlayer.Clear();
    }
}
