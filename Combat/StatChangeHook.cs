using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;            // EntityOwner, User
using ProjectM.Gameplay.Systems;   // StatChangeSystem, StatChangeReason
using Unity.Entities;              // Entity, EntityManager
using CustomKill.Combat;           // PlayerHitStore & ECSExtensions.Read<T>()
using CustomKill.Database;         // PvPStatsService
using CustomKill.Services;         // LevelService

namespace CustomKill.Combat;

[HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
public static class StatChangeHook
{
    public static void Prefix(StatChangeSystem __instance)
    {
        var changes = __instance._MergedChanges;
        Plugin.Logger.LogInfo($"[StatChangeHook] OnUpdate prefix running — {changes.Length} total changes.");
        var em = __instance.EntityManager;

        foreach (var ev in changes)
        {
            if (ev.Reason != StatChangeReason.DealDamageSystem_0) continue;
            if (ev.Change >= 0) continue;

            if (!em.HasComponent<EntityOwner>(ev.Source)) continue;
            var attackerEnt = em.GetComponentData<EntityOwner>(ev.Source).Owner;
            var victimEnt = ev.Entity;

            if (!em.HasComponent<PlayerCharacter>(attackerEnt)) continue;
            if (!em.HasComponent<PlayerCharacter>(victimEnt)) continue;

            // read the User struct directly
            var attackerUser = attackerEnt
                .Read<PlayerCharacter>()
                .UserEntity
                .Read<User>();
            var victimUser = victimEnt
                .Read<PlayerCharacter>()
                .UserEntity
                .Read<User>();

            // IDs, names, levels
            ulong aId = attackerUser.PlatformId;
            ulong vId = victimUser.PlatformId;
            string aNm = attackerUser.CharacterName.ToString();
            string vNm = victimUser.CharacterName.ToString();

            int aLvl = LevelService.Instance.GetMaxLevel(aNm);
            int vLvl = LevelService.Instance.GetMaxLevel(vNm);

            // damage
            int dmg = (int)Math.Abs(Math.Round(ev.Change));

            // skip PrefabGUID logic (no ability hash)
            int abilityHash = -1;

            // record hit for assist & store damage
            PlayerHitStore.AddHit(
                aId, aNm, aLvl,
                vId, vNm, vLvl,
                abilityHash, dmg);

            PvPStatsService.RegisterDamage(aNm, aId, dmg);
            PvPStatsService.RegisterDamageTaken(vNm, vId, dmg);
        }
    }
}
