#if UNITY_EDITOR
#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
#define FFS_ECS_DEBUG
#endif
#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    public enum QueryMethodType {
        ALL, ALL_ONLY_DISABLED, ALL_WITH_DISABLED,
        NONE, NONE_WITH_DISABLED, ANY,
        ANY_ONLY_DISABLED, ANY_WITH_DISABLED,
    }

    public struct HandleComponentsFilter : IQueryFilter {
        public List<ComponentsHandle> Handles;
        private QueryMethodType Type;

        [MethodImpl(AggressiveInlining)]
        public HandleComponentsFilter(List<ComponentsHandle> handles, QueryMethodType type) {
            Type = type;
            Handles = handles;
        }

        public ulong FilterChunk<TWorld>(uint chunkIdx) where TWorld : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:
                case QueryMethodType.ALL_ONLY_DISABLED:
                case QueryMethodType.ALL_WITH_DISABLED:
                    ulong allMask = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        allMask &= handle.HeuristicChunk(chunkIdx).NotEmptyBlocks.Value;
                    }

                    return allMask;
                case QueryMethodType.NONE:
                case QueryMethodType.NONE_WITH_DISABLED:
                    ulong noneMask = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        noneMask &= ~handle.HeuristicChunk(chunkIdx).FullBlocks.Value;
                    }

                    return noneMask;
                case QueryMethodType.ANY:
                case QueryMethodType.ANY_ONLY_DISABLED:
                case QueryMethodType.ANY_WITH_DISABLED:
                    ulong anyMask = 0;
                    foreach (var handle in Handles) {
                        anyMask |= handle.HeuristicChunk(chunkIdx).NotEmptyBlocks.Value;
                    }

                    return anyMask;
                default:
                    return ulong.MaxValue;
            }
        }

        public ulong FilterEntities<TWorld>(uint segmentIdx, byte segmentBlockIdx) where TWorld : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:
                    ulong mask = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        mask &= handle.EnabledMask(segmentIdx, segmentBlockIdx);
                    }

                    return mask;
                case QueryMethodType.ALL_ONLY_DISABLED:
                    ulong maskD = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        maskD &= handle.DisabledMask(segmentIdx, segmentBlockIdx);
                    }

                    return maskD;
                case QueryMethodType.ALL_WITH_DISABLED:
                    ulong maskA = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        maskA &= handle.AnyMask(segmentIdx, segmentBlockIdx);
                    }

                    return maskA;
                case QueryMethodType.NONE:
                    ulong nMask = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        nMask &= ~handle.EnabledMask(segmentIdx, segmentBlockIdx);
                    }

                    return nMask;
                case QueryMethodType.NONE_WITH_DISABLED:
                    ulong nMaskA = ulong.MaxValue;
                    foreach (var handle in Handles) {
                        nMaskA &= ~handle.AnyMask(segmentIdx, segmentBlockIdx);
                    }

                    return nMaskA;
                case QueryMethodType.ANY:
                    ulong aMask = 0;
                    foreach (var handle in Handles) {
                        aMask |= handle.EnabledMask(segmentIdx, segmentBlockIdx);
                    }

                    return aMask;
                case QueryMethodType.ANY_ONLY_DISABLED:
                    ulong aMaskD = 0;
                    foreach (var handle in Handles) {
                        aMaskD |= handle.DisabledMask(segmentIdx, segmentBlockIdx);
                    }

                    return aMaskD;
                case QueryMethodType.ANY_WITH_DISABLED:
                    ulong aMaskA = 0;
                    foreach (var handle in Handles) {
                        aMaskA |= handle.AnyMask(segmentIdx, segmentBlockIdx);
                    }

                    return aMaskA;
                default:
                    return ulong.MaxValue;
            }
        }

        public void PushQueryData<TWorld>(QueryData data) where TWorld : struct, IWorldType { }

        public void PopQueryData<TWorld>() where TWorld : struct, IWorldType { }
        #if FFS_ECS_DEBUG
        public void Assert<TWorld>() where TWorld : struct, IWorldType { }

        public void Block<TWorld>(int val) where TWorld : struct, IWorldType { }
        #endif

        #if FFS_ECS_BURST
        [MethodImpl(AggressiveInlining)]
        public ulong BurstFilterChunk<TWorld>(uint chunkIdx) where TWorld : struct, IWorldType => throw new Exception();

        [MethodImpl(AggressiveInlining)]
        public ulong BurstFilterEntities<TWorld>(uint segmentIdx, byte segmentBlockIdx) where TWorld : struct, IWorldType => throw new Exception();
        #endif
    }

    public struct CompositeHandleFilter : IQueryFilter {
        private List<IQueryFilter> _filters;

        public bool IsValid => _filters != null && _filters.Count > 0;

        public void Add(IQueryFilter filter) {
            _filters ??= new List<IQueryFilter>();
            _filters.Add(filter);
        }

        public void Merge(CompositeHandleFilter other) {
            if (other._filters == null) return;
            _filters ??= new List<IQueryFilter>();
            _filters.AddRange(other._filters);
        }

        public ulong FilterChunk<TWorld>(uint chunkIdx) where TWorld : struct, IWorldType {
            ulong mask = ulong.MaxValue;
            foreach (var filter in _filters) {
                mask &= filter.FilterChunk<TWorld>(chunkIdx);
            }

            return mask;
        }

        public ulong FilterEntities<TWorld>(uint segmentIdx, byte segmentBlockIdx) where TWorld : struct, IWorldType {
            ulong mask = ulong.MaxValue;
            foreach (var filter in _filters) {
                mask &= filter.FilterEntities<TWorld>(segmentIdx, segmentBlockIdx);
            }

            return mask;
        }

        #if FFS_ECS_DEBUG
        public void Block<TWorld>(int val) where TWorld : struct, IWorldType {
            foreach (var filter in _filters) {
                filter.Block<TWorld>(val);
            }
        }
        #endif

        #if FFS_ECS_BURST
        [MethodImpl(AggressiveInlining)]
        public ulong BurstFilterChunk<TWorld>(uint chunkIdx) where TWorld : struct, IWorldType => throw new Exception();

        [MethodImpl(AggressiveInlining)]
        public ulong BurstFilterEntities<TWorld>(uint segmentIdx, byte segmentBlockIdx) where TWorld : struct, IWorldType => throw new Exception();
        #endif
    }
}
#endif
#endif