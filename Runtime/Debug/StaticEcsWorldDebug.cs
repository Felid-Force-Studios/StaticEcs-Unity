#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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

    public struct EventData: IEquatable<EventData> {
        public IEvent CachedData;
        public int ReceivedIdx;
        public int InternalIdx;
        public TypeIdx TypeIdx;
        public short Version;
        public Status Status;

        public bool Equals(EventData other) {
            return TypeIdx.Value.Equals(other.TypeIdx.Value) && InternalIdx == other.InternalIdx && Version == other.Version;
        }

        public override bool Equals(object obj) {
            return obj is EventData other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(TypeIdx.Value, InternalIdx, Version);
        }
    }

    public enum Status : byte {
        Sent,
        Read,
        Suppressed
    }

    public abstract class AbstractWorldData {
        public IWorld World;
        public Type WorldTypeType;
        public uint CountWithoutDestroyed;
        public uint Capacity;
        public uint Destroyed;
        public PageRingBuffer<EventData> Events;
        public Dictionary<Type, int> EventsReceived;
        public string worldEditorName;
        public string WorldTypeTypeFullName;
        public Func<IEntity, string> WindowNameFunction;

        public abstract bool IsActual(uint idx);

        public abstract void ForAll<T>(T with, IForAll action) where T : struct, IQueryMethod;

        public abstract IEntity GetEntity(uint entIdx);

        public abstract bool FindEntityByGid(uint gid, out IEntity entity);

        public abstract void DestroyEntity(uint entIdx);
    }

    public interface IForAll {
        public bool ForAll(uint entityId);
    }

    internal class WorldData<WorldType> : AbstractWorldData where WorldType : struct, IWorldType {
        public override bool IsActual(uint idx) {
            return new World<WorldType>.Entity(idx).IsNotDestroyed();
        }

        public override void ForAll<T>(T with, IForAll action) {
            foreach (var entity in World<WorldType>.Query.Entities(with)) {
                if (!action.ForAll(entity.id - Const.ENTITY_ID_OFFSET)) {
                    return;
                }
            }
        }

        public override IEntity GetEntity(uint entIdx) {
            return new World<WorldType>.Entity(entIdx).Box();
        }

        public override bool FindEntityByGid(uint eid, out IEntity entity) {
            var chunks = World<WorldType>.Entities.Value.chunks;

            var e = new World<WorldType>.Entity(eid);
            var chunkIdx = eid >> Const.ENTITIES_IN_CHUNK_SHIFT;
            
            if (chunkIdx < chunks.Length && World<WorldType>.Entities.Value.EntityIsLoaded(e) && e.IsNotDestroyed()) {
                entity = e.Box();
                return true;
            }

            entity = default;
            return false;
        }

        public override void DestroyEntity(uint entIdx) {
            new World<WorldType>.Entity(entIdx).Destroy();
        }
        
        public void OnEventSent<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            var typeIdx = TypeIdx.Create<T>();
            
            if (typeIdx.Type.IsIgnored()) {
                return;
            }

            if (EventsReceived.TryGetValue(typeIdx.Type, out var index)) {
                index++;
            }
            EventsReceived[typeIdx.Type] = index;
            
            Events.Push(new EventData {
                TypeIdx = typeIdx,
                CachedData = null,
                ReceivedIdx = index,
                Version = World<WorldType>.Events.Pool<T>.Value._versions[value._idx],
                InternalIdx = value._idx,
                Status = Status.Sent
            });
        }
        
        public void OnEventReadAll<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Read);
        }

        public void OnEventSuppress<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            OnEventDelete(value, Status.Suppressed);
        }

        private void OnEventDelete<T>(World<WorldType>.Event<T> value, Status status) where T : struct, IEvent {
            if (TypeIdx.Create<T>().Type.IsIgnored()) {
                return;
            }
            
            var eventData = new EventData {
                TypeIdx = TypeIdx.Create<T>(),
                Version = World<WorldType>.Events.Pool<T>.Value._versions[value._idx],
                InternalIdx = value._idx,
                CachedData = value.Value,
                Status = status,
            };
            
            Events.Change(eventData, (EventData template, ref EventData item) => {
                item.CachedData = template.CachedData;
                item.Status = template.Status;
            });
        }

    }

    #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
    public sealed class StaticEcsWorldDebug<WorldType> : World<WorldType>.IWorldDebugEventListener
                                                         , World<WorldType>.IEventsDebugEventListener
        where WorldType : struct, IWorldType {
        private WorldData<WorldType> _worldData;
        internal static StaticEcsWorldDebug<WorldType> Instance;
        internal Func<IEntity, string> windowEntityNameFunction;
        private readonly int _eventHistoryPageCount;

        private StaticEcsWorldDebug(int eventHistoryCount) {
            _eventHistoryPageCount = Math.Max(eventHistoryCount, 8);
        }

        public static void Create(int eventHistoryPageCount, Func<IEntity, string> windowEntityNameFunction = null) {
            if (World<WorldType>.Status != WorldStatus.Created) {
                throw new StaticEcsException("StaticEcsWorldDebug Debug mode connection is possible only between world creation and initialization");
            }

            Instance = new StaticEcsWorldDebug<WorldType>(eventHistoryPageCount) {
                windowEntityNameFunction = windowEntityNameFunction,
            };
            World<WorldType>.DEBUG.AddWorldDebugEventListener(Instance);
            World<WorldType>.Events.AddEventsDebugEventListener(Instance);
        }

        public void OnWorldInitialized() {
            _worldData = new WorldData<WorldType> {
                World = Worlds.Get(typeof(WorldType)),
                WorldTypeType = typeof(WorldType),
                Events = new PageRingBuffer<EventData>(_eventHistoryPageCount),
                EventsReceived = new Dictionary<Type, int>(),
                WindowNameFunction = windowEntityNameFunction
            };
            
            UpdateWorldCounts();

            StaticEcsDebugData.Worlds[typeof(WorldType)] = _worldData;
        }

        public void OnWorldDestroyed() {
            World<WorldType>.DEBUG.RemoveWorldDebugEventListener(this);
            World<WorldType>.Events.RemoveEventsDebugEventListener(this);
            StaticEcsDebugData.Worlds.Remove(typeof(WorldType));
        }

        private void UpdateWorldCounts() {
            _worldData.CountWithoutDestroyed = World<WorldType>.CalculateLoadedEntitiesCount();
            _worldData.Capacity = World<WorldType>.CalculateEntitiesCapacity();
            _worldData.Destroyed = _worldData.Capacity - World<WorldType>.CalculateLoadedEntitiesCount();
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
        
        public void OnEventSent<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSent(value);
        }

        public void OnEventReadAll<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventReadAll(value);
        }

        public void OnEventSuppress<T>(World<WorldType>.Event<T> value) where T : struct, IEvent {
            _worldData.OnEventSuppress(value);
        }

    }
#endif
}
#endif
