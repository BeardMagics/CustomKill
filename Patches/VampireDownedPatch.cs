using CustomKill;
using CustomKill.Utils;
using ProjectM;

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

            // Credit-locking only: record who downed the victim
            KillCache.SetDowner(victimEntity, killerEntity);
        }

        downedEvents.Dispose();
    }
}
