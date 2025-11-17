#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
#define FFS_ECS_DEBUG
#endif
#if FFS_ECS_DEBUG || FFS_ECS_ENABLE_DEBUG_EVENTS
#define FFS_ECS_EVENTS
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs {
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1>
        where C1 : unmanaged, IComponent {
        public C1* d1;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3, C4>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent
        where C4 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public C4* d4;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3, C4, C5>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent
        where C4 : unmanaged, IComponent
        where C5 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public C4* d4;
        public C5* d5;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3, C4, C5, C6>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent
        where C4 : unmanaged, IComponent
        where C5 : unmanaged, IComponent
        where C6 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public C4* d4;
        public C5* d5;
        public C6* d6;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3, C4, C5, C6, C7>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent
        where C4 : unmanaged, IComponent
        where C5 : unmanaged, IComponent
        where C6 : unmanaged, IComponent
        where C7 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public C4* d4;
        public C5* d5;
        public C6* d6;
        public C7* d7;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public unsafe struct QueryBlocks<C1, C2, C3, C4, C5, C6, C7, C8>
        where C1 : unmanaged, IComponent
        where C2 : unmanaged, IComponent
        where C3 : unmanaged, IComponent
        where C4 : unmanaged, IComponent
        where C5 : unmanaged, IComponent
        where C6 : unmanaged, IComponent
        where C7 : unmanaged, IComponent
        where C8 : unmanaged, IComponent {
        public C1* d1;
        public C2* d2;
        public C3* d3;
        public C4* d4;
        public C5* d5;
        public C6* d6;
        public C7* d7;
        public C8* d8;
        public ulong EntitiesMask;
        public uint BlockIdx;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public readonly struct QueryBurstFunctionRunner<WorldType> where WorldType : struct, IWorldType {
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;

            var c1Data = c1.data;

            C1* d1 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];

                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled  => aMask1[blockIdx] & ~dMask1[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx],
                            _                        => aMask1[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1>>(), Allocator.Temp, 0);
                            }
                            
                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;

            C1* d1 = default;
            C2* d2 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];

                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled  => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];

                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled  => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3, C4>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3, C4>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent
            where C4 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C4>(World<WorldType>.Components<C4>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;
            ref var c4 = ref World<WorldType>.Components<C4>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;
            var c4Data = c4.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;
            C4* d4 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];

                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];
                    ref var ch4 = ref c4.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks
                                    & ch4.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;
                    var aMask4 = ch4.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;
                    var dMask4 = ch4.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled  => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx] & aMask4[blockIdx] & ~dMask4[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx] & dMask4[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx] & aMask4[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3, C4>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3, C4>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3, C4>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                                d4 = (C4*) UnsafeUtility.AddressOf(ref c4Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.d4 = d4;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3, C4, C5>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3, C4, C5>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent
            where C4 : unmanaged, IComponent
            where C5 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C4>(World<WorldType>.Components<C4>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C5>(World<WorldType>.Components<C5>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;
            ref var c4 = ref World<WorldType>.Components<C4>.Value;
            ref var c5 = ref World<WorldType>.Components<C5>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;
            var c4Data = c4.data;
            var c5Data = c5.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;
            C4* d4 = default;
            C5* d5 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];
                    
                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];
                    ref var ch4 = ref c4.chunks[chunkIdx];
                    ref var ch5 = ref c5.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks
                                    & ch4.notEmptyBlocks
                                    & ch5.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;
                    var aMask4 = ch4.entities;
                    var aMask5 = ch5.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;
                    var dMask4 = ch4.disabledEntities;
                    var dMask5 = ch5.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx] & aMask4[blockIdx] & ~dMask4[blockIdx] & aMask5[blockIdx] &
                                                       ~dMask5[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx] & dMask4[blockIdx] & dMask5[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx] & aMask4[blockIdx] & aMask5[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3, C4, C5>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3, C4, C5>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3, C4, C5>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                                d4 = (C4*) UnsafeUtility.AddressOf(ref c4Data[dataIdx][0]);
                                d5 = (C5*) UnsafeUtility.AddressOf(ref c5Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.d4 = d4;
                            cache.d5 = d5;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3, C4, C5, C6>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3, C4, C5, C6>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent
            where C4 : unmanaged, IComponent
            where C5 : unmanaged, IComponent
            where C6 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C4>(World<WorldType>.Components<C4>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C5>(World<WorldType>.Components<C5>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C6>(World<WorldType>.Components<C6>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;
            ref var c4 = ref World<WorldType>.Components<C4>.Value;
            ref var c5 = ref World<WorldType>.Components<C5>.Value;
            ref var c6 = ref World<WorldType>.Components<C6>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;
            var c4Data = c4.data;
            var c5Data = c5.data;
            var c6Data = c6.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;
            C4* d4 = default;
            C5* d5 = default;
            C6* d6 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];
                    
                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];
                    ref var ch4 = ref c4.chunks[chunkIdx];
                    ref var ch5 = ref c5.chunks[chunkIdx];
                    ref var ch6 = ref c6.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks
                                    & ch4.notEmptyBlocks
                                    & ch5.notEmptyBlocks
                                    & ch6.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;
                    var aMask4 = ch4.entities;
                    var aMask5 = ch5.entities;
                    var aMask6 = ch6.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;
                    var dMask4 = ch4.disabledEntities;
                    var dMask5 = ch5.disabledEntities;
                    var dMask6 = ch6.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx] & aMask4[blockIdx] & ~dMask4[blockIdx] & aMask5[blockIdx] &
                                                       ~dMask5[blockIdx] & aMask6[blockIdx] & ~dMask6[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx] & dMask4[blockIdx] & dMask5[blockIdx] & dMask6[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx] & aMask4[blockIdx] & aMask5[blockIdx] & aMask6[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3, C4, C5, C6>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3, C4, C5, C6>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3, C4, C5, C6>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                                d4 = (C4*) UnsafeUtility.AddressOf(ref c4Data[dataIdx][0]);
                                d5 = (C5*) UnsafeUtility.AddressOf(ref c5Data[dataIdx][0]);
                                d6 = (C6*) UnsafeUtility.AddressOf(ref c6Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.d4 = d4;
                            cache.d5 = d5;
                            cache.d6 = d6;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3, C4, C5, C6, C7>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3, C4, C5, C6, C7>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent
            where C4 : unmanaged, IComponent
            where C5 : unmanaged, IComponent
            where C6 : unmanaged, IComponent
            where C7 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C4>(World<WorldType>.Components<C4>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C5>(World<WorldType>.Components<C5>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C6>(World<WorldType>.Components<C6>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C7>(World<WorldType>.Components<C7>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;
            ref var c4 = ref World<WorldType>.Components<C4>.Value;
            ref var c5 = ref World<WorldType>.Components<C5>.Value;
            ref var c6 = ref World<WorldType>.Components<C6>.Value;
            ref var c7 = ref World<WorldType>.Components<C7>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;
            var c4Data = c4.data;
            var c5Data = c5.data;
            var c6Data = c6.data;
            var c7Data = c7.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;
            C4* d4 = default;
            C5* d5 = default;
            C6* d6 = default;
            C7* d7 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];
                    
                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];
                    ref var ch4 = ref c4.chunks[chunkIdx];
                    ref var ch5 = ref c5.chunks[chunkIdx];
                    ref var ch6 = ref c6.chunks[chunkIdx];
                    ref var ch7 = ref c7.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks
                                    & ch4.notEmptyBlocks
                                    & ch5.notEmptyBlocks
                                    & ch6.notEmptyBlocks
                                    & ch7.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;
                    var aMask4 = ch4.entities;
                    var aMask5 = ch5.entities;
                    var aMask6 = ch6.entities;
                    var aMask7 = ch7.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;
                    var dMask4 = ch4.disabledEntities;
                    var dMask5 = ch5.disabledEntities;
                    var dMask6 = ch6.disabledEntities;
                    var dMask7 = ch7.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx] & aMask4[blockIdx] & ~dMask4[blockIdx] & aMask5[blockIdx] &
                                                       ~dMask5[blockIdx] & aMask6[blockIdx] & ~dMask6[blockIdx] & aMask7[blockIdx] & ~dMask7[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx] & dMask4[blockIdx] & dMask5[blockIdx] & dMask6[blockIdx] & dMask7[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx] & aMask4[blockIdx] & aMask5[blockIdx] & aMask6[blockIdx] & aMask7[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3, C4, C5, C6, C7>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3, C4, C5, C6, C7>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3, C4, C5, C6, C7>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                                d4 = (C4*) UnsafeUtility.AddressOf(ref c4Data[dataIdx][0]);
                                d5 = (C5*) UnsafeUtility.AddressOf(ref c5Data[dataIdx][0]);
                                d6 = (C6*) UnsafeUtility.AddressOf(ref c6Data[dataIdx][0]);
                                d7 = (C7*) UnsafeUtility.AddressOf(ref c7Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.d4 = d4;
                            cache.d5 = d5;
                            cache.d6 = d6;
                            cache.d7 = d7;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(AggressiveInlining)]
        public static unsafe void Prepare<P, C1, C2, C3, C4, C5, C6, C7, C8>(ReadOnlySpan<ushort> clusters, P with, EntityStatusType entities, ComponentStatus components, out QueryBlocks<C1, C2, C3, C4, C5, C6, C7, C8>* blocks, out int blocksCount)
            where P : struct, IQueryMethod
            where C1 : unmanaged, IComponent
            where C2 : unmanaged, IComponent
            where C3 : unmanaged, IComponent
            where C4 : unmanaged, IComponent
            where C5 : unmanaged, IComponent
            where C6 : unmanaged, IComponent
            where C7 : unmanaged, IComponent
            where C8 : unmanaged, IComponent {
            #if FFS_ECS_DEBUG
            World<WorldType>.AssertNotNestedParallelQuery(World<WorldType>.WorldTypeName);
            World<WorldType>.AssertRegisteredComponent<C1>(World<WorldType>.Components<C1>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C2>(World<WorldType>.Components<C2>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C3>(World<WorldType>.Components<C3>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C4>(World<WorldType>.Components<C4>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C5>(World<WorldType>.Components<C5>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C6>(World<WorldType>.Components<C6>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C7>(World<WorldType>.Components<C7>.ComponentsTypeName);
            World<WorldType>.AssertRegisteredComponent<C8>(World<WorldType>.Components<C8>.ComponentsTypeName);
            with.Assert<WorldType>();
            #endif

            var deBruijn = Utils.DeBruijn;

            ref var c1 = ref World<WorldType>.Components<C1>.Value;
            ref var c2 = ref World<WorldType>.Components<C2>.Value;
            ref var c3 = ref World<WorldType>.Components<C3>.Value;
            ref var c4 = ref World<WorldType>.Components<C4>.Value;
            ref var c5 = ref World<WorldType>.Components<C5>.Value;
            ref var c6 = ref World<WorldType>.Components<C6>.Value;
            ref var c7 = ref World<WorldType>.Components<C7>.Value;
            ref var c8 = ref World<WorldType>.Components<C8>.Value;

            var c1Data = c1.data;
            var c2Data = c2.data;
            var c3Data = c3.data;
            var c4Data = c4.data;
            var c5Data = c5.data;
            var c6Data = c6.data;
            var c7Data = c7.data;
            var c8Data = c8.data;

            C1* d1 = default;
            C2* d2 = default;
            C3* d3 = default;
            C4* d4 = default;
            C5* d5 = default;
            C6* d6 = default;
            C7* d7 = default;
            C8* d8 = default;

            var entitiesChunks = World<WorldType>.Entities.Value.chunks;
            var entitiesClusters = World<WorldType>.Entities.Value.clusters;

            blocks = default;
            blocksCount = 0;
            var dataIdx = uint.MaxValue;

            for (var i = 0; i < clusters.Length; i++) {
                var clusterIdx = clusters[i];
                ref var cluster = ref entitiesClusters[clusterIdx];
                if (cluster.disabled) {
                    continue;
                }

                for (uint chunkMapIdx = 0; chunkMapIdx < cluster.loadedChunksCount; chunkMapIdx++) {
                    var chunkIdx = cluster.loadedChunks[chunkMapIdx];
                    
                    ref var chE = ref entitiesChunks[chunkIdx];
                    ref var ch1 = ref c1.chunks[chunkIdx];
                    ref var ch2 = ref c2.chunks[chunkIdx];
                    ref var ch3 = ref c3.chunks[chunkIdx];
                    ref var ch4 = ref c4.chunks[chunkIdx];
                    ref var ch5 = ref c5.chunks[chunkIdx];
                    ref var ch6 = ref c6.chunks[chunkIdx];
                    ref var ch7 = ref c7.chunks[chunkIdx];
                    ref var ch8 = ref c8.chunks[chunkIdx];

                    var chunkMask = chE.notEmptyBlocks
                                    & ch1.notEmptyBlocks
                                    & ch2.notEmptyBlocks
                                    & ch3.notEmptyBlocks
                                    & ch4.notEmptyBlocks
                                    & ch5.notEmptyBlocks
                                    & ch6.notEmptyBlocks
                                    & ch7.notEmptyBlocks
                                    & ch8.notEmptyBlocks;
                    with.CheckChunk<WorldType>(ref chunkMask, chunkIdx);

                    var lMask = chE.loadedEntities;
                    var aMask = chE.entities;
                    var aMask1 = ch1.entities;
                    var aMask2 = ch2.entities;
                    var aMask3 = ch3.entities;
                    var aMask4 = ch4.entities;
                    var aMask5 = ch5.entities;
                    var aMask6 = ch6.entities;
                    var aMask7 = ch7.entities;
                    var aMask8 = ch8.entities;

                    var dMask = chE.disabledEntities;
                    var dMask1 = ch1.disabledEntities;
                    var dMask2 = ch2.disabledEntities;
                    var dMask3 = ch3.disabledEntities;
                    var dMask4 = ch4.disabledEntities;
                    var dMask5 = ch5.disabledEntities;
                    var dMask6 = ch6.disabledEntities;
                    var dMask7 = ch7.disabledEntities;
                    var dMask8 = ch8.disabledEntities;

                    while (chunkMask > 0) {
                        var blockIdx = (uint) deBruijn[(int) (((chunkMask & (ulong) -(long) chunkMask) * 0x37E84A99DAE458FUL) >> 58)];
                        chunkMask &= chunkMask - 1;
                        var globalBlockIdx = blockIdx + (chunkIdx << Const.BLOCK_IN_CHUNK_SHIFT);
                        var entitiesMask = entities switch {
                            EntityStatusType.Enabled  => lMask[blockIdx] & aMask[blockIdx] & ~dMask[blockIdx],
                            EntityStatusType.Disabled => lMask[blockIdx] & dMask[blockIdx],
                            _                         => lMask[blockIdx] & aMask[blockIdx]
                        };
                        entitiesMask &= components switch {
                            ComponentStatus.Enabled => aMask1[blockIdx] & ~dMask1[blockIdx] & aMask2[blockIdx] & ~dMask2[blockIdx] & aMask3[blockIdx] & ~dMask3[blockIdx] & aMask4[blockIdx] & ~dMask4[blockIdx] & aMask5[blockIdx] &
                                                       ~dMask5[blockIdx] & aMask6[blockIdx] & ~dMask6[blockIdx] & aMask7[blockIdx] & ~dMask7[blockIdx] & aMask8[blockIdx] & ~dMask8[blockIdx],
                            ComponentStatus.Disabled => dMask1[blockIdx] & dMask2[blockIdx] & dMask3[blockIdx] & dMask4[blockIdx] & dMask5[blockIdx] & dMask6[blockIdx] & dMask7[blockIdx] & dMask8[blockIdx],
                            _                        => aMask1[blockIdx] & aMask2[blockIdx] & aMask3[blockIdx] & aMask4[blockIdx] & aMask5[blockIdx] & aMask6[blockIdx] & aMask7[blockIdx] & aMask8[blockIdx],
                        };
                        with.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, (int) blockIdx);

                        if (entitiesMask > 0) {
                            if (blocksCount == 0) {
                                var size = sizeof(QueryBlocks<C1, C2, C3, C4, C5, C6, C7, C8>) * World<WorldType>.Entities.Value.chunks.Length << Const.BLOCK_IN_CHUNK_SHIFT;
                                blocks = (QueryBlocks<C1, C2, C3, C4, C5, C6, C7, C8>*) UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<QueryBlocks<C1, C2, C3, C4, C5, C6, C7, C8>>(), Allocator.Temp, 0);
                            }

                            if (dataIdx != globalBlockIdx >> Const.DATA_QUERY_SHIFT) {
                                dataIdx = globalBlockIdx >> Const.DATA_QUERY_SHIFT;
                                d1 = (C1*) UnsafeUtility.AddressOf(ref c1Data[dataIdx][0]);
                                d2 = (C2*) UnsafeUtility.AddressOf(ref c2Data[dataIdx][0]);
                                d3 = (C3*) UnsafeUtility.AddressOf(ref c3Data[dataIdx][0]);
                                d4 = (C4*) UnsafeUtility.AddressOf(ref c4Data[dataIdx][0]);
                                d5 = (C5*) UnsafeUtility.AddressOf(ref c5Data[dataIdx][0]);
                                d6 = (C6*) UnsafeUtility.AddressOf(ref c6Data[dataIdx][0]);
                                d7 = (C7*) UnsafeUtility.AddressOf(ref c7Data[dataIdx][0]);
                                d8 = (C8*) UnsafeUtility.AddressOf(ref c8Data[dataIdx][0]);
                            }

                            ref var cache = ref blocks[blocksCount++];
                            cache.d1 = d1;
                            cache.d2 = d2;
                            cache.d3 = d3;
                            cache.d4 = d4;
                            cache.d5 = d5;
                            cache.d6 = d6;
                            cache.d7 = d7;
                            cache.d8 = d8;
                            cache.EntitiesMask = entitiesMask;
                            cache.BlockIdx = globalBlockIdx;
                        }
                    }
                }
            }
        }
    }
}