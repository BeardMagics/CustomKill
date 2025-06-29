using Unity.Entities;
using ProjectM;                        // for StatChangeEvent
using ProjectM.Gameplay.Systems;       // StatChangeReason
using CustomKill.Combat;               // PlayerHitStore & StatChangeHook

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

        // No per-frame work needed
        public override void OnUpdate() { }

        private void HandleStatChanged(StatChangeEvent ev)
        {
            // No assist linking here anymore
            // All hit logging is done in StatChangeHook already
        }
    }
}
