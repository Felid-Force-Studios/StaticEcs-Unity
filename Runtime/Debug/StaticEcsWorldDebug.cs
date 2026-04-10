#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;

namespace FFS.Libraries.StaticEcs.Unity {
    internal static class TypeDescriptorData {
        public static ushort Value = 1;
        public static readonly Dictionary<ushort, Type> Types = new();
    }

    internal static class TypeDescriptor<T> {
        public static readonly ushort Value;

        static TypeDescriptor() {
            Value = TypeDescriptorData.Value++;
            TypeDescriptorData.Types[Value] = typeof(T);
        }
    }

    public struct TypeIdx {
        public ushort Value;

        public static TypeIdx Create<T>() {
            return new TypeIdx {
                Value = TypeDescriptor<T>.Value
            };
        }

        public Type Type => TypeDescriptorData.Types[Value];
    }

    public struct EventData : IEquatable<EventData> {
        public IEvent CachedData;
        public int ReceivedIdx;
        public int InternalIdx;
        public TypeIdx TypeIdx;
        public EventStatus EventStatus;

        public bool Equals(EventData other) {
            return TypeIdx.Value.Equals(other.TypeIdx.Value) && InternalIdx == other.InternalIdx;
        }

        public override bool Equals(object obj) {
            return obj is EventData other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(TypeIdx.Value, InternalIdx);
        }
    }

    public abstract class AbstractWorldData {
        public WorldHandle Handle;
        public PageRingBuffer<EventData> Events;
        public Dictionary<Type, int> EventsReceived;
        public Func<EntityGID, string> WindowNameFunction;
    }

    internal class WorldData<TWorld> : AbstractWorldData, World<TWorld>.IEventsDebugEventListener where TWorld : struct, IWorldType {
        private SpinLock _eventListenerLock = new SpinLock(false);

        public void OnEventSent<T>(World<TWorld>.Event<T> value) where T : struct, IEvent {
            var typeIdx = TypeIdx.Create<T>();

            if (typeIdx.Type.IsIgnored()) {
                return;
            }

            var lockTaken = false;
            _eventListenerLock.Enter(ref lockTaken);

            if (EventsReceived.TryGetValue(typeIdx.Type, out var index)) {
                index++;
            }

            EventsReceived[typeIdx.Type] = index;

            Events.Push(new EventData {
                TypeIdx = typeIdx,
                CachedData = null,
                ReceivedIdx = index,
                InternalIdx = value.EventIdx,
                EventStatus = EventStatus.Sent
            });

            _eventListenerLock.Exit();
        }

        public void OnEventReadAll<T>(World<TWorld>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, EventStatus.Read);
        }

        public void OnEventSuppress<T>(World<TWorld>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, EventStatus.Suppressed);
        }

        private void OnEventDelete<T>(World<TWorld>.Event<T> value, EventStatus eventStatus) where T : struct, IEvent {
            if (TypeIdx.Create<T>().Type.IsIgnored()) {
                return;
            }

            var eventData = new EventData {
                TypeIdx = TypeIdx.Create<T>(),
                InternalIdx = value.EventIdx,
                CachedData = value.Value,
                EventStatus = eventStatus,
            };

            Events.Change(eventData, (EventData template, ref EventData item) => {
                item.CachedData = template.CachedData;
                item.EventStatus = template.EventStatus;
            });
        }
    }
}
#endif