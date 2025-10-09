using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    
    #if UNITY_EDITOR
    public sealed class StaticEcsDebugData {
        public static readonly Dictionary<Type, AbstractWorldData> Worlds = new();
        public static readonly Dictionary<Type, ((ISystem system, short order, int idx)[] systems, int count, Type worldType)> Systems = new();
    }
    #endif

    public abstract class EcsDebug<WorldType> where WorldType : struct, IWorldType {
        public static void AddSystem<SystemsType>() where SystemsType : struct, ISystemsType {
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (!World<WorldType>.Systems<SystemsType>.IsInitialized()) {
                throw new StaticEcsException("StaticEcsWorldDebug Debug mode connection is possible only when systems initialized");
            }
            StaticEcsDebugData.Systems[typeof(SystemsType)] = (World<WorldType>.Systems<SystemsType>._allSystems, World<WorldType>.Systems<SystemsType>._allSystemsCount, typeof(WorldType));
            #endif
            #endif
        }
        
        public static void AddWorld(int eventHistoryCount = 8192, Func<IEntity, string> windowEntityNameFunction = null) {
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            StaticEcsWorldDebug<WorldType>.Create(eventHistoryCount, windowEntityNameFunction);
            #endif
            #endif
        }
    }

    public abstract class AutoRegister<WorldType> where WorldType : struct, IWorldType {
        public static void Apply() {
            var methods = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .SelectMany(asm => asm.GetTypes())
                                   .SelectMany(type => type.GetMethods(
                                                   BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                                   .Where(m => m.GetCustomAttribute<StaticEcsAutoRegistrationAttribute>() != null && m.GetCustomAttribute<StaticEcsAutoRegistrationAttribute>().WorldType == typeof(WorldType));

            foreach (var method in methods) {
                method.Invoke(null, null);
            }
        }
    }
}