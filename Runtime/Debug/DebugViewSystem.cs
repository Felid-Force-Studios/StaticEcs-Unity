#if UNITY_EDITOR
#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FFS.Libraries.StaticEcs.Unity {

    public enum DebugCommandType : byte {
        SetComponent,
        Delete,
        EnableComponent,
        DisableComponent,
        SetTag,
        EnableEntity,
        DisableEntity,
        DestroyEntity,
    }

    public struct DebugCommand {
        public DebugCommandType Type;
        public EntityGID EntityGid;
        public Type TargetType;
        public IComponent Value;
    }

    public class EntitySnapshot<TWorld> where TWorld : struct, IWorldType {
        public World<TWorld>.Entity[] Entities;
        public int Count;

        public EntitySnapshot(int capacity) {
            Entities = new World<TWorld>.Entity[capacity];
            Count = 0;
        }
    }

    public class DebugViewSystem<TWorld> : ISystem where TWorld : struct, IWorldType {
        private readonly ConcurrentQueue<DebugCommand> _commandQueue = new();
        private EntitySnapshot<TWorld> _snapshot;
        private CompositeHandleFilter _filter;
        private ComponentsHandle? _sortComponentHandle;
        private volatile int _maxEntityResult = 100;
        private readonly long _refreshIntervalMs = 100;
        private readonly Stopwatch _stopwatch = new();

        public int MaxEntityResult {
            get => _maxEntityResult;
            set => _maxEntityResult = value;
        }

        public void EnqueueCommand(DebugCommand command) {
            _commandQueue.Enqueue(command);
        }

        public EntitySnapshot<TWorld> ReadSnapshot() {
            return _snapshot;
        }

        public void SetFilter(CompositeHandleFilter filter) {
            _filter = filter;
        }

        public void SetSortHandle(ComponentsHandle? componentHandle) {
            _sortComponentHandle = componentHandle;
        }

        public void Init() {
            _snapshot = new EntitySnapshot<TWorld>(256);
            _stopwatch.Start();
        }

        public bool UpdateIsActive() {
            if (_stopwatch.ElapsedMilliseconds < _refreshIntervalMs) {
                return false;
            }
            _stopwatch.Restart();
            return true;
        }

        public void Update() {
            ProcessCommands();
            RefreshSnapshot();
        }

        public void Destroy() { }

        private void ProcessCommands() {
            while (_commandQueue.TryDequeue(out var cmd)) {
                if (!cmd.EntityGid.TryUnpack<TWorld>(out var entity)) {
                    continue;
                }

                switch (cmd.Type) {
                    case DebugCommandType.SetComponent:
                        if (World<TWorld>.Data.Handle.TryGetComponentsHandle(cmd.TargetType, out var setHandle)) {
                            setHandle.SetRaw(entity.ID, cmd.Value);
                        }
                        break;
                    case DebugCommandType.Delete:
                        if (World<TWorld>.Data.Handle.TryGetComponentsHandle(cmd.TargetType, out var delHandle)) {
                            delHandle.Delete(entity.ID);
                        }
                        break;
                    case DebugCommandType.EnableComponent:
                        if (World<TWorld>.Data.Handle.TryGetComponentsHandle(cmd.TargetType, out var enHandle)) {
                            enHandle.Enable(entity.ID);
                        }
                        break;
                    case DebugCommandType.DisableComponent:
                        if (World<TWorld>.Data.Handle.TryGetComponentsHandle(cmd.TargetType, out var disHandle)) {
                            disHandle.Disable(entity.ID);
                        }
                        break;
                    case DebugCommandType.SetTag:
                        if (World<TWorld>.Data.Handle.TryGetComponentsHandle(cmd.TargetType, out var tagSetHandle)) {
                            tagSetHandle.Set(entity.ID);
                        }
                        break;
                    case DebugCommandType.EnableEntity:
                        entity.Enable();
                        break;
                    case DebugCommandType.DisableEntity:
                        entity.Disable();
                        break;
                    case DebugCommandType.DestroyEntity:
                        entity.Destroy();
                        break;
                }
            }
        }

        private void RefreshSnapshot() {
            var max = _maxEntityResult;
            _snapshot.Count = 0;
            if (_snapshot.Entities.Length < max) {
                Array.Resize(ref _snapshot.Entities, max);
            }

            var hasSortHandle = _sortComponentHandle.HasValue;
            var filter = _filter;

            if (hasSortHandle) {
                var sortFilter = BuildSortFilter(filter);
                foreach (var entity in World<TWorld>.Query(sortFilter).Entities(EntityStatusType.Any)) {
                    if (_snapshot.Count >= max) break;
                    _snapshot.Entities[_snapshot.Count++] = entity;
                }
            }

            if (_snapshot.Count < max) {
                var sortedCount = _snapshot.Count;
                if (filter.IsValid) {
                    foreach (var entity in World<TWorld>.Query(filter).Entities(EntityStatusType.Any)) {
                        if (_snapshot.Count >= max) break;
                        if (!IsAlreadyInSnapshot(entity, sortedCount)) {
                            _snapshot.Entities[_snapshot.Count++] = entity;
                        }
                    }
                } else if (!hasSortHandle) {
                    foreach (var entity in World<TWorld>.Query().Entities(EntityStatusType.Any)) {
                        if (_snapshot.Count >= max) break;
                        _snapshot.Entities[_snapshot.Count++] = entity;
                    }
                } else {
                    foreach (var entity in World<TWorld>.Query().Entities(EntityStatusType.Any)) {
                        if (_snapshot.Count >= max) break;
                        if (!HasSortHandle(entity)) {
                            _snapshot.Entities[_snapshot.Count++] = entity;
                        }
                    }
                }
            }
        }

        private CompositeHandleFilter BuildSortFilter(CompositeHandleFilter baseFilter) {
            var sortFilter = new CompositeHandleFilter();
            if (baseFilter.IsValid) {
                sortFilter.Merge(baseFilter);
            }
            if (_sortComponentHandle.HasValue) {
                var handles = new System.Collections.Generic.List<ComponentsHandle>(1) { _sortComponentHandle.Value };
                sortFilter.Add(new HandleComponentsFilter(handles, QueryMethodType.ALL));
            }
            return sortFilter;
        }

        private bool HasSortHandle(World<TWorld>.Entity entity) {
            if (_sortComponentHandle.HasValue) return _sortComponentHandle.Value.Has(entity.ID);
            return false;
        }

        private bool IsAlreadyInSnapshot(World<TWorld>.Entity entity, int sortedCount) {
            if (sortedCount == 0) return false;
            if (!HasSortHandle(entity)) return false;
            return true;
        }

    }
}
#endif
#endif
