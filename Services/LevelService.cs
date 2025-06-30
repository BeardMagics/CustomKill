using System;
using Unity.Entities;
using ProjectM;
using ProjectM.Network;
using UnityEngine;
using CustomKill.Database;

namespace CustomKill.Services
{
    public class LevelService
    {
        public static LevelService Instance = new();

        public int GetMaxLevel(string playerName)
        {
            var collection = DatabaseWrapper.Instance.Collection;
            var stats = collection.FindById(playerName);
            return stats?.MaxGearScore ?? 0;
        }

        public void UpdatePlayerLevel(Entity userEntity)
        {
            var entityManager = Core.Server.EntityManager;

            if (!entityManager.HasComponent<User>(userEntity)) return;

            var user = entityManager.GetComponentData<User>(userEntity);
            var charEntity = user.LocalCharacter._Entity;

            if (!entityManager.HasComponent<Equipment>(charEntity)) return;

            var equipment = entityManager.GetComponentData<Equipment>(charEntity);
            var currentLevel = Mathf.RoundToInt(equipment.ArmorLevel + equipment.SpellLevel + equipment.WeaponLevel);

            var charName = user.CharacterName.ToString();

            var collection = DatabaseWrapper.Instance.Collection;
            var stats = collection.FindById(charName);

            if (stats == null)
            {
                stats = new PlayerStats
                {
                    Name = charName,
                    SteamID = user.PlatformId,
                    MaxGearScore = currentLevel
                };
                collection.Insert(stats);
                Debug.Log($"[CustomKill] Inserted new PlayerStats for {charName} with MaxGearScore: {currentLevel}");
            }
            else if (currentLevel > (stats.MaxGearScore ?? 0))
            {
                stats.MaxGearScore = currentLevel;
                collection.Update(stats);
                Debug.Log($"[CustomKill] Updated MaxGearScore for {charName}: {currentLevel}");
            }
        }
    }
}
