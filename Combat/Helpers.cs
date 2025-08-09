using System;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace CustomKill.Combat;

public static class Helpers
{
    // Gives access to the “Server” world and its EntityManager
    public static World Server { get; } = GetWorld("Server");

    private static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
            if (world.Name == name)
                return world;
        return null;
    }
    public static Entity GetCharacterFromSteamID(ulong steamID)
    {
        var entityManager = Server.EntityManager;
        var userQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<User>());
        NativeArray<Entity> users = userQuery.ToEntityArray(Allocator.Temp);

        Entity result = Entity.Null;

        foreach (var userEntity in users)
        {
            var user = entityManager.GetComponentData<User>(userEntity);
            if (user.PlatformId == steamID)
            {
                result = user.LocalCharacter._Entity;
                break;
            }
        }

        users.Dispose();
        return result;
    }
}