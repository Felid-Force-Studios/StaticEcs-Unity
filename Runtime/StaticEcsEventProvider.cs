using System;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [DefaultExecutionOrder(short.MaxValue)]
    public partial class StaticEcsEventProvider : AbstractStaticEcsProvider {
        [SerializeReference, HideInInspector] public IEvent EventTemplate;
        [HideInInspector] public RuntimeEvent RuntimeEvent = RuntimeEvent.Empty;
        [HideInInspector] public IEvent EventCache;

        protected virtual void Awake() {
            if (UsageType == UsageType.OnAwake) {
                SendEvent();
            }
        }

        protected virtual void Start() {
            if (UsageType == UsageType.OnStart) {
                SendEvent();
            }
        }

        public bool SendEvent(bool onCreateEvent = true) {
            #if DEBUG
            if (EventTemplate == null) {
                Debug.LogWarning($"You're trying to send event in an uninitialized template {WorldEditorName}");
                return false;
            }
            
            if (World == null) {
                Debug.LogWarning($"You're trying to send event in an uninitialized world {WorldEditorName}");
                return false;
            }
            #endif
            
            if (World.Events().TryGetPool(EventTemplate.GetType(), out var pool)) {
                if (pool.ReceiversCount() > 0) {
                    pool.AddRaw(EventTemplate);
                    RuntimeEvent = new RuntimeEvent {
                        InternalIdx = pool.Last(),
                        Version = pool.Version(pool.Last()),
                        Type = EventTemplate.GetType()
                    };
                    EventCache = pool.GetRaw(pool.Last());
                } else {
                    Debug.LogWarning($"No registered receivers found for event type {EventTemplate.GetType().Name}");
                }
            } else {
                throw new StaticEcsException("Event pool not registered");
            }

            if (onCreateEvent) {
                OnCreate();
            }

            return true;
        }
    }
    
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [Serializable]
    public struct RuntimeEvent {
        public static RuntimeEvent Empty = new() {
            InternalIdx = -1,
            Version = 0,
            Type = null
        };
            
        public Type Type;
        public int InternalIdx;
        public ushort Version;

        public bool IsEmpty() => InternalIdx == -1;
    }
}
