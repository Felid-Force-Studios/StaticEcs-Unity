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
        public int Count;
        public int CountWithoutDestroyed;
        public int Capacity;
        public int Destroyed;
        public int DestroyedCapacity;
        public EventData[] Events;
        public int EventsCount;
        public int EventsStart;
        public string worldEditorName;
        public string WorldTypeTypeFullName;

        public abstract bool IsActual(int idx);

        public abstract void ForAll<T>(T with, IForAll action) where T : struct, IPrimaryQueryMethod;

        public abstract IEntity GetEntity(int entIdx);

        public abstract void DestroyEntity(int entIdx);
    }

    public interface IForAll {
        public bool ForAll(int entityId);
    }

    internal class WorldData<WorldType> : AbstractWorldData where WorldType : struct, IWorldType {
        public override bool IsActual(int idx) {
            return Ecs<WorldType>.Entity.FromIdx(idx).IsActual();
        }

        public override void ForAll<T>(T with, IForAll action) {
            foreach (var entity in Ecs<WorldType>.World.QueryEntities.For(with)) {
                if (!action.ForAll(entity._id)) {
                    return;
                }
            }
        }

        public override IEntity GetEntity(int entIdx) {
            return Ecs<WorldType>.Entity.FromIdx(entIdx);
        }

        public override void DestroyEntity(int entIdx) {
            Ecs<WorldType>.Entity.FromIdx(entIdx).Destroy();
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        public void OnEventSent<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            if (EventsCount == Events.Length) {
                Array.Resize(ref Events, Events.Length << 1);
            }

            Events[EventsCount++] = new EventData {
                TypeIdx = TypeIdx.Create<T>(),
                CachedData = null,
                Version = Ecs<WorldType>.Events.Pool<T>.Value._versions[value._idx],
                InternalIdx = value._idx,
                Status = Status.Sent
            };
        }
        
        public void OnEventReadAll<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Read);
        }

        public void OnEventSuppress<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Suppressed);
        }

        private void OnEventDelete<T>(Ecs<WorldType>.Event<T> value, Status status) where T : struct, IEvent {
            var type = typeof(T);
            for (var index = 0; index < EventsCount; index++) {
                ref var val = ref Events[index];
                if (val.Version == Ecs<WorldType>.Events.Pool<T>.Value._versions[value._idx] && val.InternalIdx == value._idx && val.TypeIdx.Type == type) {
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

    public sealed class StaticEcsWorldDebug<WorldType> : Ecs<WorldType>.IWorldDebugEventListener
                                                         #if !FFS_ECS_DISABLE_EVENTS
                                                         , Ecs<WorldType>.IEventsDebugEventListener
                                                         #endif
        where WorldType : struct, IWorldType {
        private WorldData<WorldType> _worldData;
        internal static StaticEcsWorldDebug<WorldType> Instance;
        private int _maxDeletedEventHistoryCount;

        private StaticEcsWorldDebug(int maxDeletedEventHistoryCount) {
            _maxDeletedEventHistoryCount = Math.Max(maxDeletedEventHistoryCount, 128);
        }

        public static void Create(int maxDeletedEventHistoryCount = 128) {
            if (Ecs<WorldType>.World.Status != WorldStatus.Created) {
                throw new Exception("StaticEcsWorldDebug Debug mode connection is possible only between world creation and initialization");
            }

            Instance = new StaticEcsWorldDebug<WorldType>(maxDeletedEventHistoryCount);
            Ecs<WorldType>.World.AddWorldDebugEventListener(Instance);
            #if !FFS_ECS_DISABLE_EVENTS
            Ecs<WorldType>.Events.AddEventsDebugEventListener(Instance);
            #endif
        }

        public void OnWorldInitialized() {
            _worldData = new WorldData<WorldType> {
                World = Worlds.Get(typeof(WorldType)),
                Count = Ecs<WorldType>.World.EntitiesCount(),
                CountWithoutDestroyed = Ecs<WorldType>.World.EntitiesCountWithoutDestroyed(),
                Capacity = Ecs<WorldType>.World.EntitiesCapacity(),
                Destroyed = Ecs<WorldType>.World._deletedEntitiesCount,
                DestroyedCapacity = Ecs<WorldType>.World._deletedEntities.Length,
                WorldTypeType = typeof(WorldType),
                Events = new EventData[_maxDeletedEventHistoryCount * 3]
            };

            StaticEcsDebugData.Worlds[typeof(WorldType)] = _worldData;
        }

        public void OnWorldDestroyed() {
            Ecs<WorldType>.World.RemoveWorldDebugEventListener(this);
            #if !FFS_ECS_DISABLE_EVENTS
            Ecs<WorldType>.Events.RemoveEventsDebugEventListener(this);
            #endif
            StaticEcsDebugData.Worlds.Remove(typeof(WorldType));
        }

        private void UpdateWorldCounts() {
            _worldData.Count = Ecs<WorldType>.World.EntitiesCount();
            _worldData.CountWithoutDestroyed = Ecs<WorldType>.World.EntitiesCountWithoutDestroyed();
            _worldData.Capacity = Ecs<WorldType>.World.EntitiesCapacity();
            _worldData.Destroyed = Ecs<WorldType>.World._deletedEntitiesCount;
            _worldData.DestroyedCapacity = Ecs<WorldType>.World._deletedEntities.Length;
        }

        public void OnWorldResized(int capacity) {
            UpdateWorldCounts();
        }

        public void OnEntityCreated(Ecs<WorldType>.Entity entity) {
            UpdateWorldCounts();
        }

        public void OnEntityDestroyed(Ecs<WorldType>.Entity entity) {
            UpdateWorldCounts();
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        public void OnEventSent<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSent(value);
        }

        public void OnEventReadAll<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventReadAll(value);
        }

        public void OnEventSuppress<T>(Ecs<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSuppress(value);
        }
        #endif

    }
}
#endif