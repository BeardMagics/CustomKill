﻿using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using CustomKill.Services;
using UnityEngine;
using Stunlock.Network;

namespace CustomKill.Patches
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class PlayerConnectPatch
    {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            try
            {
                var entityManager = __instance.EntityManager;

                if (!__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out var userIndex))
                    return;

                var serverClients = __instance._ApprovedUsersLookup;
                if (userIndex < 0 || userIndex >= serverClients.Length)
                    return;

                var serverClient = serverClients[userIndex];
                var userEntity = serverClient.UserEntity;

                if (!entityManager.HasComponent<User>(userEntity)) return;

                var user = entityManager.GetComponentData<User>(userEntity);
                var characterName = user.CharacterName.ToString();
                if (string.IsNullOrWhiteSpace(characterName)) return;

                LevelService.Instance.UpdatePlayerLevel(userEntity);
                Debug.Log($"[CustomKill] {characterName} connected. Level updated.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomKill] Error during OnUserConnected: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
