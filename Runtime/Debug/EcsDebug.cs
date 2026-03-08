using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

[assembly: InternalsVisibleTo("FFS.StaticEcs.Unity.Editor")]

namespace FFS.Libraries.StaticEcs.Unity {
    #if UNITY_EDITOR
    internal sealed class StaticEcsDebugData {
        internal static readonly Dictionary<Type, AbstractWorldData> Worlds = new();
        internal static readonly Dictionary<Type, (SystemData[] systems, int count, Type worldType)> Systems = new();
    }
    #endif

    public abstract class EcsDebug<TWorld> where TWorld : struct, IWorldType {
        
        #if UNITY_EDITOR
        #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
        public static DebugViewSystem<TWorld> DebugViewSystem { get; private set; }
        #endif
        #endif

        public static void AddSystem<TSystemsType>() where TSystemsType : struct, ISystemsType {
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (!World<TWorld>.Systems<TSystemsType>.IsInitialized) {
                throw new InvalidOperationException("StaticEcsWorldDebug Debug mode connection is possible only when systems initialized");
            }
            StaticEcsDebugData.Systems[typeof(TSystemsType)] = (World<TWorld>.Systems<TSystemsType>.AllSystems, World<TWorld>.Systems<TSystemsType>.AllSystemsCount, typeof(TWorld));
            #endif
            #endif
        }

        public static void AddWorld<TSystemsType>(int eventHistoryCount = 8192, Func<EntityGID, string> windowEntityNameFunction = null, short systemOrder = short.MaxValue) where TSystemsType : struct, ISystemsType {
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (World<TWorld>.Status == WorldStatus.NotCreated) {
                throw new InvalidOperationException("StaticEcsWorldDebug Debug mode connection is possible only after world creation");
            }

            var worldData = new WorldData<TWorld> {
                Handle = World<TWorld>.Data.Handle,
                Events = new PageRingBuffer<EventData>(Math.Max(eventHistoryCount, 8)),
                EventsReceived = new Dictionary<Type, int>(),
                WindowNameFunction = windowEntityNameFunction ?? (gid => $"Entity {gid.EntityID}") ,
            };

            StaticEcsDebugData.Worlds[typeof(TWorld)] = worldData;
            World<TWorld>.Data.Instance.EventListener = worldData;
            DebugViewSystem = new DebugViewSystem<TWorld>();
            World<TWorld>.Systems<TSystemsType>.Add(DebugViewSystem, systemOrder);
            #endif
            #endif
        }

        public static void RemoveWorld() {
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            DebugViewSystem = null;
            StaticEcsDebugData.Worlds.Remove(typeof(TWorld));
            World<TWorld>.Data.Instance.EventListener = default;
            #endif
            #endif
        }
    }
}
