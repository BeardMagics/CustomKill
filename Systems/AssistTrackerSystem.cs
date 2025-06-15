using Unity.Entities;
using ProjectM;                        // for StatChangeEvent
using ProjectM.Gameplay.Systems;       // StatChangeReason
using CustomKill.Combat;               // StatChangeHook
using CustomKill.Utils;                // KillCache

namespace CustomKill.Systems
{
    public partial class AssistTrackerSystem : SystemBase
    {
        public override void OnCreate()
        {
            base.OnCreate();
            StatChangeHook.OnStatChanged += HandleStatChanged;
        }

        public override void OnDestroy()
        {
            StatChangeHook.OnStatChanged -= HandleStatChanged;
            base.OnDestroy();
        }

        // Remove per-frame polling updates
        public override void OnUpdate() { }

        private void HandleStatChanged(StatChangeEvent ev)
        {
            // Re-checks filters to ensure processing only relevant events and stat changes
            if (ev.Reason != StatChangeReason.DealDamageSystem_0 || ev.Change >= 0)
                return;

            var em = EntityManager;

            if (!em.HasComponent<EntityOwner>(ev.Source))
                return;

            var attackerEnt = em.GetComponentData<EntityOwner>(ev.Source).Owner;
            var victimEnt = ev.Entity;

            if (attackerEnt == Entity.Null
             || victimEnt == Entity.Null
             || attackerEnt == victimEnt)
                return;

            if (!em.HasComponent<PlayerCharacter>(attackerEnt)
             || !em.HasComponent<PlayerCharacter>(victimEnt))
                return;

            // Assit logic from KillCache - Hopefully it triggers correctly
            KillCache.AddAssist(victimEnt, attackerEnt);
        }
    }
}
