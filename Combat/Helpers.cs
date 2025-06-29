using System;
using ProjectM;
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

}
