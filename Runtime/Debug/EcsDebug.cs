using System;
using System.Collections.Generic;
#if ENABLE_IL2CPP
using System;
using Unity.IL2CPP.CompilerServices;
#endif

#if UNITY_EDITOR
namespace FFS.Libraries.StaticEcs.Unity {
    
    public sealed class StaticEcsDebugData {
        public static readonly Dictionary<Type, AbstractWorldData> Worlds = new();
        public static readonly Dictionary<Type, ((ISystem system, short order)[] systems, int count, Type worldType)> Systems = new();
    }
    
    public abstract class EcsDebug<WorldType> where WorldType : struct, IWorldType {
        public static void AddSystem<SystemsType>() where SystemsType : struct, ISystemsType {
            #if UNITY_EDITOR
            StaticEcsDebugData.Systems[typeof(SystemsType)] = (Ecs<WorldType>.Systems<SystemsType>._allSystems, Ecs<WorldType>.Systems<SystemsType>._allSystemsCount, typeof(WorldType));
            #endif
        }
        
        public static void AddWorld(int maxDeletedEventHistoryCount = 200) {
            #if UNITY_EDITOR
            StaticEcsWorldDebug<WorldType>.Create(maxDeletedEventHistoryCount);
            #endif
        }
    }
}
#endif

#if ENABLE_IL2CPP
namespace Unity.IL2CPP.CompilerServices {
    enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            Option = option;
            Value = value;
        }
    }
}
#endif