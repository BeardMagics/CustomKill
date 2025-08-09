using System;
using CustomKill.Database;
using CustomKill.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

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
                var steamID = user.PlatformId;

                if (!string.IsNullOrWhiteSpace(characterName))
                {
                    Debug.Log($"[CustomKill] New player detected: {characterName}.");
                }

                // Clan registration logic
                Entity clanEntity = user.ClanEntity._Entity;
                if (clanEntity == Entity.Null || !entityManager.HasComponent<ClanTeam>(clanEntity)) continue;

                var clanTeam = entityManager.GetComponentData<ClanTeam>(clanEntity);
                var clanName = clanTeam.Name.ToString();
                var recordId = $"{steamID}:{clanName}";

                var existing = DatabaseWrapper.Instance.ClanMembersCollection.FindById(recordId);
                if (existing != null) continue;

                var record = new ClanMemberRecord
                {
                    Id = recordId,
                    SteamID = steamID,
                    ClanName = clanName,
                    JoinedAt = DateTime.UtcNow // store in UTC for proper cutoff
                };

                DatabaseWrapper.Instance.ClanMembersCollection.Upsert(record);
                Plugin.Logger.LogInfo($"[CustomKill] Registered {characterName} to clan '{clanName}' at {record.JoinedAt}.");
            }

            entities.Dispose();
        }
    }
}
