using CustomKill;
using CustomKill.Combat;  // PlayerHitStore
using CustomKill.Services;
using CustomKill.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;

namespace CustomKill.Patches
{
    [HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
    public static class VampireDownedPatch
    {
        public static void Prefix(VampireDownedServerEventSystem __instance)
        {
            var entityManager = Core.Server.EntityManager;
            var downedEvents = __instance.__query_1174204813_0.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var entity in downedEvents)
            {
                if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, entityManager, out var victimEntity))
                    continue;

                var downBuff = entityManager.GetComponentData<VampireDownedBuff>(entity);

                if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, entityManager, out var killerEntity))
                    continue;

                if (!entityManager.HasComponent<PlayerCharacter>(victimEntity) || !entityManager.HasComponent<PlayerCharacter>(killerEntity))
                    continue;

                // Lock the downer for final kill resolution
                KillCache.SetDowner(victimEntity, killerEntity);

                //  Guarantee the downer is in PlayerHitStore too (force hit)
                var attackerUser = killerEntity.Read<PlayerCharacter>().UserEntity.Read<User>();
                var victimUser = victimEntity.Read<PlayerCharacter>().UserEntity.Read<User>();

                PlayerHitStore.AddHit(
                    attackerUser.PlatformId,
                    attackerUser.CharacterName.ToString(),
                    LevelService.Instance.GetMaxLevel(attackerUser.CharacterName.ToString()),
                    victimUser.PlatformId,
                    victimUser.CharacterName.ToString(),
                    LevelService.Instance.GetMaxLevel(victimUser.CharacterName.ToString()),
                    -1, 1
                );
            }

            downedEvents.Dispose();
        }
    }
}
