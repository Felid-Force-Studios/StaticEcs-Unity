using System;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [Serializable]
    public struct RuntimeEvent {
        public static RuntimeEvent Empty = new() {
            InternalIdx = -1,
            Status = default,
            Type = null
        };

        public Type Type;
        public EventStatus Status;
        public int InternalIdx;

        public bool IsEmpty() => InternalIdx == -1;
    }

    public enum EventStatus : byte {
        Sent, Read, Suppressed
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public abstract partial class StaticEcsEventProvider<TWorld> : AbstractStaticEcsProvider
        where TWorld : struct, IWorldType {

        [SerializeReference, HideInInspector]
        private IEvent eventTemplate;

        public IEvent EventTemplate {
            get => eventTemplate;
            set => eventTemplate = value;
        }

        public RuntimeEvent RuntimeEvent { get; set; } = RuntimeEvent.Empty;

        public IEvent EventCache { get; set; }

        protected void Awake() {
            if (UsageType == UsageType.OnAwake) {
                SendEvent();
            }
        }

        protected void Start() {
            if (UsageType == UsageType.OnStart) {
                SendEvent();
            }
        }

        public virtual bool SendEvent(bool onCreateEvent = true) {
            #if DEBUG
            if (EventTemplate == null) {
                Debug.LogWarning($"You're trying to send event in an uninitialized template {typeof(TWorld).Name}");
                return false;
            }

            if (World<TWorld>.Status != WorldStatus.Initialized) {
                Debug.LogWarning($"You're trying to send event in an uninitialized world {typeof(TWorld).Name}");
                return false;
            }
            #endif

            var handle = World<TWorld>.Data.Handle;
            if (handle.TryGetEventsHandle(EventTemplate.GetType(), out var eventsHandle)) {
                if (eventsHandle.ReceiversCount() > 0) {
                    eventsHandle.AddRaw(EventTemplate);
                    RuntimeEvent = new RuntimeEvent {
                        InternalIdx = eventsHandle.Last(),
                        Status = EventStatus.Read,
                        Type = EventTemplate.GetType()
                    };
                    EventCache = eventsHandle.GetRaw(eventsHandle.Last());
                } else {
                    Debug.LogWarning($"No registered receivers found for event type {EventTemplate.GetType().Name}");
                }
            } else {
                throw new InvalidOperationException("Event pool not registered");
            }

            if (onCreateEvent) {
                OnCreate();
            }

            return true;
        }
    }
}