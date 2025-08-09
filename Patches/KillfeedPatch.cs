using System;
using CustomKill;
using CustomKill.Services;
using CustomKill.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System.Linq;
using UnityEngine;
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
        private static EntityQuery _deathQuery;

        static KillfeedPatch()
        {
            var entityManager = Core.Server.EntityManager;
            _deathQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<DeathEvent>());
        }

        public static void Postfix(DeathEventListenerSystem __instance)
        {
            var entityManager = Core.Server.EntityManager;
            var entities = _deathQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                if (!entityManager.HasComponent<DeathEvent>(entity))
                    continue;

                var deathEvent = entityManager.GetComponentData<DeathEvent>(entity);
                var victim = deathEvent.Died;

                var downer = KillCache.GetDowner(victim);
                var killer = downer != Entity.Null ? downer : deathEvent.Killer;

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
                RegisterClanIfMissing(killerUserEntity, entityManager);
                RegisterClanIfMissing(victimUserEntity, entityManager);


                var victimName = victimUser.CharacterName.ToString();
                var killerName = killerUser.CharacterName.ToString();

                if (string.IsNullOrWhiteSpace(victimName) ||
                    string.IsNullOrWhiteSpace(killerName) ||
                    victimName == killerName)
                    continue;

                LevelService.Instance.UpdatePlayerLevel(killerUserEntity);
                LevelService.Instance.UpdatePlayerLevel(victimUserEntity);

                PvPStatsService.BufferPvPStat(killerName, killerUser.PlatformId, true, false, false);
                PvPStatsService.BufferPvPStat(victimName, victimUser.PlatformId, false, true, false);

                int killerLevel;
                int victimLevel;

                if (KillfeedSettings.LevelTrackingMode.Value == 1)
                {
                    killerLevel = LevelService.Instance.GetMaxLevel(killerName);
                    victimLevel = LevelService.Instance.GetMaxLevel(victimName);
                }
                else
                {
                    killerLevel = GetLiveGearLevel(entityManager, killerUserEntity);
                    victimLevel = GetLiveGearLevel(entityManager, victimUserEntity);
                }

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

                    var detailsInGame = assisters.Select(e => $"<color={ColorSettings.Stats_AssistsColor.Value}>{e.Value.Name}</color> ({e.Value.Level})");
                    msg += $"\n<color=#FFFFFF>[Assist(s): {string.Join(", ", detailsInGame)}]</color>";

                    foreach (var assister in assisters)
                    {
                        PvPStatsService.BufferPvPStat(
                            assister.Value.Name, assister.Key,
                            false, false, true);

                        var assisterEntity = Helpers.GetCharacterFromSteamID(assister.Key);
                        if (assisterEntity != Entity.Null &&
                            entityManager.HasComponent<PlayerCharacter>(assisterEntity))
                        {
                            var assisterUserEntity = entityManager.GetComponentData<PlayerCharacter>(assisterEntity).UserEntity;
                            RegisterClanIfMissing(assisterUserEntity, entityManager);
                        }
                    }

                }

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

        private static int GetLiveGearLevel(EntityManager entityManager, Entity userEntity)
        {
            if (!entityManager.HasComponent<User>(userEntity)) return 0;

            var user = entityManager.GetComponentData<User>(userEntity);
            var charEntity = user.LocalCharacter._Entity;

            if (!entityManager.HasComponent<Equipment>(charEntity)) return 0;

            var equipment = entityManager.GetComponentData<Equipment>(charEntity);
            return Mathf.RoundToInt(equipment.ArmorLevel + equipment.SpellLevel + equipment.WeaponLevel);
        }
        private static void RegisterClanIfMissing(Entity userEntity, EntityManager entityManager)
        {
            if (!entityManager.HasComponent<User>(userEntity)) return;

            var user = entityManager.GetComponentData<User>(userEntity);
            var steamID = user.PlatformId;
            var name = user.CharacterName.ToString();

            Entity clanEntity = user.ClanEntity._Entity;
            if (clanEntity == Entity.Null || !entityManager.HasComponent<ClanTeam>(clanEntity)) return;

            var clanTeam = entityManager.GetComponentData<ClanTeam>(clanEntity);
            var clanName = clanTeam.Name.ToString();
            var recordId = $"{steamID}:{clanName}";

            var existing = DatabaseWrapper.Instance.ClanMembersCollection.FindById(recordId);
            if (existing != null) return;

            var record = new ClanMemberRecord
            {
                Id = recordId,
                SteamID = steamID,
                ClanName = clanName,
                JoinedAt = DateTime.UtcNow
            };

            DatabaseWrapper.Instance.ClanMembersCollection.Upsert(record);
            Plugin.Logger.LogInfo($"[ClanFallback] Registered {name} to clan '{clanName}' during PvP.");
        }
    }
}
