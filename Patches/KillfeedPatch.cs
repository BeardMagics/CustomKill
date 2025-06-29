using CustomKill;
using CustomKill.Services;
using CustomKill.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.StunDebug;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using CustomKill.Database;

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

                // 1) grab all the cached hit/assist data *before* clearing
                var downer = KillCache.GetDowner(victim);
                var assisters = KillCache.GetAssistants(victim);

                // 2) decide who is the killer
                var killer = downer != Entity.Null ? downer : deathEvent.Killer;

                // only clear *after* we've captured downer + assisters
                KillCache.Clear(victim);

                // skip if either party isn't a player
                if (!entityManager.HasComponent<PlayerCharacter>(victim) ||
                    !entityManager.HasComponent<PlayerCharacter>(killer))
                    continue;

                var victimUser = entityManager.GetComponentData<PlayerCharacter>(victim).UserEntity;
                var killerUser = entityManager.GetComponentData<PlayerCharacter>(killer).UserEntity;

                if (!entityManager.HasComponent<User>(victimUser) ||
                    !entityManager.HasComponent<User>(killerUser))
                    continue;

                var victimData = entityManager.GetComponentData<User>(victimUser);
                var killerData = entityManager.GetComponentData<User>(killerUser);

                var victimName = victimData.CharacterName.ToString();
                var killerName = killerData.CharacterName.ToString();

                if (string.IsNullOrWhiteSpace(victimName) ||
                    string.IsNullOrWhiteSpace(killerName) ||
                    victimName == killerName)
                    continue;

                // Update levels
                LevelService.Instance.UpdatePlayerLevel(killerUser);
                LevelService.Instance.UpdatePlayerLevel(victimUser);

                // Register kill & death
                PvPStatsService.RegisterPvPStats(
                    killerName, killerData.PlatformId,
                    isKill: true, isDeath: false, isAssist: false);
                PvPStatsService.RegisterPvPStats(
                    victimName, victimData.PlatformId,
                    isKill: false, isDeath: true, isAssist: false);

                var killerLevel = LevelService.Instance.GetMaxLevel(killerName);
                var victimLevel = LevelService.Instance.GetMaxLevel(victimName);

                var killerClan = TruncateClan(GetClanName(entityManager, killerData));
                var victimClan = TruncateClan(GetClanName(entityManager, victimData));

                // Determine kill validity & colors
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

                FixedString512Bytes chatMsg = msg;
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref chatMsg);

                // Build Discord message
                string discordMessage = $"⚔️ [{killerClan}] **{killerName}** ({killerLevel}) " +
                                        $"killed **{victimName}** ({victimLevel})";

                // 3) process any assists we captured earlier
                if (assisters != null && assisters.Count > 0)
                {
                    var details = assisters.Select(e =>
                    {
                        var userComp = entityManager.GetComponentData<User>(
                            entityManager.GetComponentData<PlayerCharacter>(e).UserEntity);
                        var name = userComp.CharacterName.ToString();
                        var level = LevelService.Instance.GetMaxLevel(name);
                        return $"*{name}* ({level})";
                    });

                    discordMessage += $" -- Assisters: {string.Join(", ", details)}";
                 
                    //Add assist details to in-game chat but remove discord formatting
                    var detailsIngame = assisters.Select(e =>
                    {
                        var userComp = entityManager.GetComponentData<User>(
                            entityManager.GetComponentData<PlayerCharacter>(e).UserEntity);
                        var name = userComp.CharacterName.ToString();
                        var level = LevelService.Instance.GetMaxLevel(name);
                        return $"{name} ({level})";
                    });

                    msg += $" [Assist(s): {string.Join(", ", detailsIngame)}]";

                    // register each assist
                    foreach (var assister in assisters)
                    {
                        var userComp = entityManager.GetComponentData<User>(
                            entityManager.GetComponentData<PlayerCharacter>(assister).UserEntity);
                        var name = userComp.CharacterName.ToString();
                        var steamId = userComp.PlatformId;

                        PvPStatsService.RegisterPvPStats(
                            name, steamId,
                            isKill: false, isDeath: false, isAssist: true);
                    }
                }

                DiscordBroadcaster.SendKillMessage(discordMessage);
            }

            entities.Dispose();
        }

        private static string GetClanName(EntityManager em, User user)
        {
            var clanEntity = user.ClanEntity._Entity;
            if (clanEntity != Entity.Null && em.HasComponent<ClanTeam>(clanEntity))
            {
                return em.GetComponentData<ClanTeam>(clanEntity).Name.ToString();
            }
            return "-";
        }

        private static string TruncateClan(string name)
        {
            if (string.IsNullOrEmpty(name)) return "---";
            return name.Length <= 3
                ? name.ToUpper()
                : name.Substring(0, 3).ToUpper();
        }

        private static bool IsKillAllowed(int killerLevel, int victimLevel)
        {
            int diff = killerLevel - victimLevel;
            if (killerLevel >= 91)
                return diff <= KillfeedSettings.MaxLevelGapHigh.Value;
            return diff <= KillfeedSettings.MaxLevelGapNormal.Value;
        }
    }
}
