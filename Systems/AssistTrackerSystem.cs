using Unity.Entities;
using ProjectM;
using CustomKill.Utils;

namespace CustomKill.Systems
{
    public partial class AssistTrackerSystem : SystemBase
    {
        public override void OnUpdate()
        {
            var entityManager = EntityManager;

            var damageQuery = GetEntityQuery(ComponentType.ReadOnly<DamageTakenEvent>());
            var damageEntities = damageQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
            var damageEvents = damageQuery.ToComponentDataArray<DamageTakenEvent>(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < damageEntities.Length; i++)
            {
                var damageEvent = damageEvents[i];
                var attacker = damageEvent.Source;
                var victim = damageEvent.Entity;

                if (attacker == Entity.Null || victim == Entity.Null || attacker == victim)
                    continue;

                if (entityManager.HasComponent<PlayerCharacter>(attacker) &&
                    entityManager.HasComponent<PlayerCharacter>(victim))
                {
                    KillCache.AddAssist(victim, attacker);
                }
            }

            damageEntities.Dispose();
            damageEvents.Dispose();
        }
    }
}
