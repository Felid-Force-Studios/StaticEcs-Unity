#if UNITY_EDITOR
using System;
using System.Collections.Generic;

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

    public struct EventData {
        public TypeIdx TypeIdx;
        public IEvent CachedData;
        public int InternalIdx;
        public short Version;
        public Status Status;
    }

    public enum Status : byte {
        Sent,
        Read,
        Suppressed
    }

    public abstract class AbstractWorldData {
        public IWorld World;
        public Type WorldTypeType;
        public uint Count;
        public uint CountWithoutDestroyed;
        public uint Capacity;
        public uint Destroyed;
        public uint DestroyedCapacity;
        public EventData[] Events;
        public int EventsCount;
        public int EventsStart;
        public string worldEditorName;
        public string WorldTypeTypeFullName;

        public abstract bool IsActual(uint idx);

        public abstract void ForAll<T>(T with, IForAll action) where T : struct, IPrimaryQueryMethod;

        public abstract IEntity GetEntity(uint entIdx);

        public abstract void DestroyEntity(uint entIdx);
    }

    public interface IForAll {
        public bool ForAll(uint entityId);
    }

    internal class WorldData<WorldType> : AbstractWorldData where WorldType : struct, IWorldType {
        public override bool IsActual(uint idx) {
            return World<WorldType>.Entity.FromIdx(idx).IsActual();
        }

        public override void ForAll<T>(T with, IForAll action) {
            foreach (var entity in World<WorldType>.QueryEntities.For(with)) {
                if (!action.ForAll(entity._id)) {
                    return;
                }
            }
        }

        public override IEntity GetEntity(uint entIdx) {
            return World<WorldType>.Entity.FromIdx(entIdx);
        }

        public override void DestroyEntity(uint entIdx) {
            World<WorldType>.Entity.FromIdx(entIdx).Destroy();
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        public void OnEventSent<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            if (EventsCount == Events.Length) {
                Array.Resize(ref Events, Events.Length << 1);
            }

            Events[EventsCount++] = new EventData {
                TypeIdx = TypeIdx.Create<T>(),
                CachedData = null,
                Version = World<WorldType>.Events.Pool<T>.Value._versions[value._idx],
                InternalIdx = value._idx,
                Status = Status.Sent
            };
        }
        
        public void OnEventReadAll<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Read);
        }

        public void OnEventSuppress<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Suppressed);
        }

        private void OnEventDelete<T>(World<WorldType>.Event<T> value, Status status) where T : struct, IEvent {
            var type = typeof(T);
            for (var index = 0; index < EventsCount; index++) {
                ref var val = ref Events[index];
                if (val.Version == World<WorldType>.Events.Pool<T>.Value._versions[value._idx] && val.InternalIdx == value._idx && val.TypeIdx.Type == type) {
                    val.CachedData = value.Value;
                    val.Status = status;
                    if ((EventsCount - EventsStart) >= 256 && Events[EventsStart].Status is Status.Read or Status.Suppressed) {
                        EventsStart++;
                    }

                    if (EventsStart == 128) {
                        EventsCount -= 128;
                        EventsStart -= 128;
                        Array.Copy(Events, 128, Events, 0, EventsCount);
                    }

                    break;
                }
            }
        }
        #endif

    }

    public sealed class StaticEcsWorldDebug<WorldType> : World<WorldType>.IWorldDebugEventListener
                                                         #if !FFS_ECS_DISABLE_EVENTS
                                                         , World<WorldType>.IEventsDebugEventListener
                                                         #endif
        where WorldType : struct, IWorldType {
        private WorldData<WorldType> _worldData;
        internal static StaticEcsWorldDebug<WorldType> Instance;
        private int _maxDeletedEventHistoryCount;

        private StaticEcsWorldDebug(int maxDeletedEventHistoryCount) {
            _maxDeletedEventHistoryCount = Math.Max(maxDeletedEventHistoryCount, 128);
        }

        public static void Create(int maxDeletedEventHistoryCount = 128) {
            if (World<WorldType>.Status != WorldStatus.Created) {
                throw new Exception("StaticEcsWorldDebug Debug mode connection is possible only between world creation and initialization");
            }

            Instance = new StaticEcsWorldDebug<WorldType>(maxDeletedEventHistoryCount);
            World<WorldType>.AddWorldDebugEventListener(Instance);
            #if !FFS_ECS_DISABLE_EVENTS
            World<WorldType>.Events.AddEventsDebugEventListener(Instance);
            #endif
        }

        public void OnWorldInitialized() {
            _worldData = new WorldData<WorldType> {
                World = Worlds.Get(typeof(WorldType)),
                Count = World<WorldType>.EntitiesCount(),
                CountWithoutDestroyed = World<WorldType>.EntitiesCountWithoutDestroyed(),
                Capacity = World<WorldType>.EntitiesCapacity(),
                Destroyed = World<WorldType>.Entity.deletedEntitiesCount,
                DestroyedCapacity = (uint) World<WorldType>.Entity.deletedEntities.Length,
                WorldTypeType = typeof(WorldType),
                Events = new EventData[_maxDeletedEventHistoryCount * 3]
            };

            StaticEcsDebugData.Worlds[typeof(WorldType)] = _worldData;
        }

        public void OnWorldDestroyed() {
            World<WorldType>.RemoveWorldDebugEventListener(this);
            #if !FFS_ECS_DISABLE_EVENTS
            World<WorldType>.Events.RemoveEventsDebugEventListener(this);
            #endif
            StaticEcsDebugData.Worlds.Remove(typeof(WorldType));
        }

        private void UpdateWorldCounts() {
            _worldData.Count = World<WorldType>.EntitiesCount();
            _worldData.CountWithoutDestroyed = World<WorldType>.EntitiesCountWithoutDestroyed();
            _worldData.Capacity = World<WorldType>.EntitiesCapacity();
            _worldData.Destroyed = World<WorldType>.Entity.deletedEntitiesCount;
            _worldData.DestroyedCapacity = (uint) World<WorldType>.Entity.deletedEntities.Length;
        }

        public void OnWorldResized(uint capacity) {
            UpdateWorldCounts();
        }

        public void OnEntityCreated(World<WorldType>.Entity entity) {
            UpdateWorldCounts();
        }

        public void OnEntityDestroyed(World<WorldType>.Entity entity) {
            UpdateWorldCounts();
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        public void OnEventSent<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSent(value);
        }

        public void OnEventReadAll<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventReadAll(value);
        }

        public void OnEventSuppress<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSuppress(value);
        }
        #endif

    }
}
#endif