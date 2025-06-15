using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;            // EntityOwner, User
using ProjectM.Gameplay.Systems;   // StatChangeSystem, StatChangeReason
using Unity.Entities;              // EntityManager
using CustomKill.Combat;           // PlayerHitStore & ECSExtensions.Read<T>()
using CustomKill.Database;         // PvPStatsService
using CustomKill.Services;         // LevelService

namespace CustomKill.Combat
{
    public static class StatChangeHook
    {
 
        // Fires once per qualifying StatChangeEvent
      
        public static event Action<StatChangeEvent> OnStatChanged;

        // Changed to nested class so it can be invoked from other systems
        [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
        public static class Patch
        {
            public static void Prefix(StatChangeSystem __instance)
            {
                var changes = __instance._MergedChanges;
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

                    // Hit / Level / Damage logic
                    var attackerUser = attackerEnt
                        .Read<PlayerCharacter>()
                        .UserEntity
                        .Read<User>();
                    var victimUser = victimEnt
                        .Read<PlayerCharacter>()
                        .UserEntity
                        .Read<User>();

                    ulong aId = attackerUser.PlatformId;
                    ulong vId = victimUser.PlatformId;
                    string aNm = attackerUser.CharacterName.ToString();
                    string vNm = victimUser.CharacterName.ToString();
                    int aLvl = LevelService.Instance.GetMaxLevel(aNm);
                    int vLvl = LevelService.Instance.GetMaxLevel(vNm);
                    int dmg = (int)Math.Abs(Math.Round(ev.Change));
                    int abilityHash = -1;

                    PlayerHitStore.AddHit(aId, aNm, aLvl,
                                         vId, vNm, vLvl,
                                         abilityHash, dmg);

                    PvPStatsService.RegisterDamage(aNm, aId, dmg);
                    PvPStatsService.RegisterDamageTaken(vNm, vId, dmg);

                    //Raises the event for subscribers
                    OnStatChanged?.Invoke(ev);
                }
            }
        }
    }
}
