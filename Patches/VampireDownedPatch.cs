using CustomKill.Utils;
using CustomKill.Database;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CustomKill.Patches
{
    [HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
    public static class VampireDownedPatch
    {
        public static void Prefix(VampireDownedServerEventSystem __instance)
        {
            var entityManager = Core.Server.EntityManager;
            var downedEvents = __instance.__query_1174204813_0.ToEntityArray(Allocator.Temp); // downed players

            foreach (var entity in downedEvents)
            {
                if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, entityManager, out var victimEntity))
                    continue;

                var downBuff = entityManager.GetComponentData<VampireDownedBuff>(entity);

                if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, entityManager, out var killerEntity))
                    continue;

                if (!entityManager.HasComponent<PlayerCharacter>(victimEntity) || !entityManager.HasComponent<PlayerCharacter>(killerEntity))
                    continue;

                var victimUserEntity = entityManager.GetComponentData<PlayerCharacter>(victimEntity).UserEntity;
                var killerUserEntity = entityManager.GetComponentData<PlayerCharacter>(killerEntity).UserEntity;

                if (!entityManager.HasComponent<User>(victimUserEntity) || !entityManager.HasComponent<User>(killerUserEntity))
                    continue;

                var victimUser = entityManager.GetComponentData<User>(victimUserEntity);
                var killerUser = entityManager.GetComponentData<User>(killerUserEntity);

                var victimName = victimUser.CharacterName.ToString();
                var killerName = killerUser.CharacterName.ToString();

                // Register the killer's stats
                PvPStatsService.RegisterPvPStats(
                    killerName,
                    killerUser.PlatformId,
                    isKill: true,
                    isDeath: false,
                    isAssist: false
                );

                // Register the victim's death
                PvPStatsService.RegisterPvPStats(
                    victimName,
                    victimUser.PlatformId,
                    isKill: false,
                    isDeath: true,
                    isAssist: false
                );

                // Set the killer as the last downer (used internally)
                KillCache.SetDowner(victimEntity, killerEntity);

                // Process assists
                var assisters = KillCache.GetAssists(victimEntity, killerEntity);

                foreach (var assister in assisters)
                {
                    if (!entityManager.HasComponent<PlayerCharacter>(assister)) continue;

                    var assisterUserEntity = entityManager.GetComponentData<PlayerCharacter>(assister).UserEntity;
                    if (!entityManager.HasComponent<User>(assisterUserEntity)) continue;

                    var assisterUser = entityManager.GetComponentData<User>(assisterUserEntity);
                    PvPStatsService.RegisterPvPStats(
                        assisterUser.CharacterName.ToString(),
                        assisterUser.PlatformId,
                        isKill: false,
                        isDeath: false,
                        isAssist: true
                    );
                }

                // Optional cleanup
                KillCache.ClearAssists(victimEntity);
            }

            downedEvents.Dispose();
        }
    }
}
