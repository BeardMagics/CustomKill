using CustomKill.Database;
using CustomKill.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using UnityEngine;

namespace CustomKill.Patches
{
    [HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
    public class PlayerCreationPatch
    {
        public static void Postfix(Destroy_TravelBuffSystem __instance)
        {
            var entityManager = __instance.EntityManager;
            var entities = __instance.__query_615927226_0.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                // Replace this with the correct GUID once confirmed
                var guid = entityManager.GetComponentData<PrefabGUID>(entity);
                if (guid.GuidHash != 722466953) continue;

                var owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!entityManager.HasComponent<PlayerCharacter>(owner)) continue;

                var userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                if (!entityManager.HasComponent<User>(userEntity)) continue;

                var user = entityManager.GetComponentData<User>(userEntity);
                var characterName = user.CharacterName.ToString();

                if (!string.IsNullOrWhiteSpace(characterName))
                {
                    Debug.Log($"[CustomKill] New player detected: {characterName}.");
                }
            }

            entities.Dispose();
        }
    }
}
