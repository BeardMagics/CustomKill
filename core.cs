﻿using ProjectM;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace CustomKill
{
    public static class Core
    {
        public static World Server { get; } = GetWorld("Server") ?? throw new Exception("Server World not found. Mod installation incorrect or missing resources.");
        public static EntityManager EntityManager => Server.EntityManager;
        public static PrefabCollectionSystem PrefabCollection { get; } = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                    return world;
            }
            return null;
        }

        public static void LogException(Exception e, [CallerMemberName] string caller = null)
        {
            Debug.LogError($"[CustomKill] Exception in {caller}: {e.Message}\n{e.StackTrace}");
        }
    }
}
