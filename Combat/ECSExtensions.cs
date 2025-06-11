using CustomKill.Combat;  // <— Helpers lives here
using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using System;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace CustomKill.Combat;  // file-scoped namespace

#pragma warning disable CS8500
public static class ECSExtensions
{
    public unsafe static T Read<T>(this Entity entity) where T : struct
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        void* rawPtr = Helpers.Server.EntityManager
            .GetComponentDataRawRO(entity, ct.TypeIndex);
        return Marshal.PtrToStructure<T>(new IntPtr(rawPtr));
    }

    public static bool Has<T>(this Entity entity) where T : struct
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        return Helpers.Server.EntityManager.HasComponent(entity, ct);
    }
}
#pragma warning restore CS8500
