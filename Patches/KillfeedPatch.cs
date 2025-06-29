using CustomKill;
using CustomKill.Services;
using CustomKill.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using CustomKill.Database;
using CustomKill.Combat;
using CustomKill.Config;

namespace CustomKill.Patches
{
    [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
    public static class KillfeedPatch
    {
        public static void Postfix(DeathEventListenerSystem __instance)
        {
            var entityManager = Core.Server.EntityManager;
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<DeathEvent>());
            var entities = query.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                if (!entityManager.HasComponent<DeathEvent>(entity))
                    continue;

                var deathEvent = entityManager.GetComponentData<DeathEvent>(entity);
                var victim = deathEvent.Died;

                // Always prefer the locked downer
                var downer = KillCache.GetDowner(victim);
                var killer = downer != Entity.Null ? downer : deathEvent.Killer;

                // Clear downer lock after use
                KillCache.Clear(victim);

                if (!entityManager.HasComponent<PlayerCharacter>(victim) ||
                    !entityManager.HasComponent<PlayerCharacter>(killer))
                    continue;

                var victimUserEntity = entityManager.GetComponentData<PlayerCharacter>(victim).UserEntity;
                var killerUserEntity = entityManager.GetComponentData<PlayerCharacter>(killer).UserEntity;

                if (!entityManager.HasComponent<User>(victimUserEntity) ||
                    !entityManager.HasComponent<User>(killerUserEntity))
                    continue;

                var victimUser = entityManager.GetComponentData<User>(victimUserEntity);
                var killerUser = entityManager.GetComponentData<User>(killerUserEntity);

                var victimName = victimUser.CharacterName.ToString();
                var killerName = killerUser.CharacterName.ToString();

                if (string.IsNullOrWhiteSpace(victimName) ||
                    string.IsNullOrWhiteSpace(killerName) ||
                    victimName == killerName)
                    continue;

                // Update levels for both
                LevelService.Instance.UpdatePlayerLevel(killerUserEntity);
                LevelService.Instance.UpdatePlayerLevel(victimUserEntity);

                // Update stats for kill & death
                PvPStatsService.RegisterPvPStats(killerName, killerUser.PlatformId, isKill: true, isDeath: false, isAssist: false);
                PvPStatsService.RegisterPvPStats(victimName, victimUser.PlatformId, isKill: false, isDeath: true, isAssist: false);

                var killerLevel = LevelService.Instance.GetMaxLevel(killerName);
                var victimLevel = LevelService.Instance.GetMaxLevel(victimName);

                var killerClan = TruncateClan(GetClanName(entityManager, killerUser));
                var victimClan = TruncateClan(GetClanName(entityManager, victimUser));

                var killAllowed = IsKillAllowed(killerLevel, victimLevel);
                var levelColor = killAllowed
                    ? KillfeedSettings.AllowedLevelColor.Value
                    : KillfeedSettings.ForbiddenLevelColor.Value;

                var msg = KillfeedSettings.KillMessageFormat.Value
                    .Replace("{Killer}", killerName)
                    .Replace("{Victim}", victimName)
                    .Replace("{KillerClan}", killerClan)
                    .Replace("{VictimClan}", victimClan)
                    .Replace("{KillerLevel}", killerLevel.ToString())
                    .Replace("{VictimLevel}", victimLevel.ToString())
                    .Replace("{LevelColor}", levelColor)
                    .Replace("{KillerNameColor}", KillfeedSettings.KillerNameColor.Value)
                    .Replace("{VictimNameColor}", KillfeedSettings.VictimNameColor.Value)
                    .Replace("{ClanTagColor}", KillfeedSettings.ClanTagColor.Value);

                // Lookup assist data *before* sending final message
                var victimSteamId = victimUser.PlatformId;
                var killerSteamId = killerUser.PlatformId;

                var assisters = PlayerHitStore.GetRecentAttackersWithLvl(victimSteamId)
                    .Where(x => x.Key != killerSteamId)
                    .ToList();

                string discordMessage = $"⚔️ [{killerClan}] **{killerName}** ({killerLevel}) killed **{victimName}** ({victimLevel})";

                if (assisters.Count > 0)
                {
                    var details = assisters.Select(e => $"*{e.Value.Name}* ({e.Value.Level})");
                    discordMessage += $"\n-- Assisters: {string.Join(", ", details)}";

                    var detailsInGame = assisters.Select(e => $"<color={ColorSettings.Stats_AssistsColor}>{e.Value.Name}</color> ({e.Value.Level})");
                    msg += $"\n<color=#FFFFFF>[Assist(s): {string.Join(", ", detailsInGame)}]</color>";

                    foreach (var assister in assisters)
                    {
                        PvPStatsService.RegisterPvPStats(
                            assister.Value.Name, assister.Key,
                            isKill: false, isDeath: false, isAssist: true);
                    }
                }

                // send fully built chat message
                FixedString512Bytes chatMsg = msg;
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref chatMsg);

                DiscordBroadcaster.SendKillMessage(discordMessage);
            }

            entities.Dispose();
        }

        private static string GetClanName(EntityManager em, User user)
        {
            var clanEntity = user.ClanEntity._Entity;
            return clanEntity != Entity.Null && em.HasComponent<ClanTeam>(clanEntity)
                ? em.GetComponentData<ClanTeam>(clanEntity).Name.ToString()
                : "-";
        }

        private static string TruncateClan(string name)
        {
            return string.IsNullOrEmpty(name)
                ? "---"
                : (name.Length <= 3 ? name.ToUpper() : name.Substring(0, 3).ToUpper());
        }

        private static bool IsKillAllowed(int killerLevel, int victimLevel)
        {
            int diff = killerLevel - victimLevel;
            return killerLevel >= 91
                ? diff <= KillfeedSettings.MaxLevelGapHigh.Value
                : diff <= KillfeedSettings.MaxLevelGapNormal.Value;
        }
    }
}
