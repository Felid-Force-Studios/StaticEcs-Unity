using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    [Flags]
    public enum ComponentOrTagKind {
        Component = 1, Tag = 2, Multi = 4 | Component,
        Link = 8 | Component, Links = 16 | Component,
    }

    public static class ComponentOrTagKindExtensions {
        public static bool IsComponent(this ComponentOrTagKind kind) => (kind & ComponentOrTagKind.Component) != 0;

        public static bool IsTag(this ComponentOrTagKind kind) => (kind & ComponentOrTagKind.Tag) != 0;

        public static bool IsMulti(this ComponentOrTagKind kind) => (kind & ComponentOrTagKind.Multi) == ComponentOrTagKind.Multi;

        public static bool IsLink(this ComponentOrTagKind kind) => (kind & ComponentOrTagKind.Link) == ComponentOrTagKind.Link;

        public static bool IsLinks(this ComponentOrTagKind kind) => (kind & ComponentOrTagKind.Links) == ComponentOrTagKind.Links;
    }

    public interface IComponentOrTagProvider {
        ComponentOrTagKind Kind { get; }
        Type ComponentType { get; }

        void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType;
    }

    [Serializable]
    public class ComponentProvider : IComponentOrTagProvider {
        [SerializeReference]
        public IComponent value;

        public ComponentOrTagKind Kind => ComponentOrTagKind.Component;
        public Type ComponentType => value?.GetType();

        public void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType {
            if (value == null) return;
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (deferred && Application.isPlaying && EcsDebug<TWorld>.DebugViewSystem != null) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = entity.GID,
                    TargetType = value.GetType(),
                    Value = value,
                });
                return;
            }
            #endif
            #endif
            ref var world = ref World<TWorld>.Data.Handle;
            if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                handle.SetRaw(entity.ID, value);
            }
        }
    }

    [Serializable]
    public class TagProvider : IComponentOrTagProvider {
        [SerializeReference]
        public ITag value;

        public ComponentOrTagKind Kind => ComponentOrTagKind.Tag;
        public Type ComponentType => value?.GetType();

        public void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType {
            if (value == null) return;
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (deferred && Application.isPlaying && EcsDebug<TWorld>.DebugViewSystem != null) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetTag,
                    EntityGid = entity.GID,
                    TargetType = value.GetType(),
                });
                return;
            }
            #endif
            #endif
            ref var world = ref World<TWorld>.Data.Handle;
            if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                handle.Set(entity.ID);
            }
        }
    }

    [Serializable]
    internal class LinkProvider : IComponentOrTagProvider {
        [SerializeReference]
        public ILinkComponent value;
        [SerializeField]
        public AbstractStaticEcsEntityProvider target;

        public ComponentOrTagKind Kind => ComponentOrTagKind.Link;
        public Type ComponentType => value?.GetType();

        public void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType {
            if (value == null) return;
            if (target != null) return;
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (deferred && Application.isPlaying && EcsDebug<TWorld>.DebugViewSystem != null) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = entity.GID,
                    TargetType = value.GetType(),
                    Value = (IComponent) value,
                });
                return;
            }
            #endif
            #endif
            ref var world = ref World<TWorld>.Data.Handle;
            if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                handle.SetRaw(entity.ID, (IComponentOrTag) value);
            }
        }
    }

    [Serializable]
    internal class LinksProvider : IComponentOrTagProvider {
        [SerializeReference]
        public ILinksComponent value;
        [SerializeField]
        public List<AbstractStaticEcsEntityProvider> targets = new();

        public ComponentOrTagKind Kind => ComponentOrTagKind.Links;
        public Type ComponentType => value?.GetType();

        public void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType {
            if (value == null) return;
            if (targets != null && targets.Count > 0) return;
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (deferred && Application.isPlaying && EcsDebug<TWorld>.DebugViewSystem != null) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = entity.GID,
                    TargetType = value.GetType(),
                    Value = (IComponent) value,
                });
                return;
            }
            #endif
            #endif
            ref var world = ref World<TWorld>.Data.Handle;
            if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                handle.SetRaw(entity.ID, (IComponentOrTag) value);
            }
        }
    }

    [Serializable]
    public class MultiProvider : IComponentOrTagProvider {
        [SerializeReference]
        public IComponent value;

        public ComponentOrTagKind Kind => ComponentOrTagKind.Multi;
        public Type ComponentType => value?.GetType();

        public void Apply<TWorld>(World<TWorld>.Entity entity, bool deferred = false) where TWorld : struct, IWorldType {
            if (value == null) return;
            #if UNITY_EDITOR
            #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
            if (deferred && Application.isPlaying && EcsDebug<TWorld>.DebugViewSystem != null) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = entity.GID,
                    TargetType = value.GetType(),
                    Value = value,
                });
                return;
            }
            #endif
            #endif
            ref var world = ref World<TWorld>.Data.Handle;
            if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                handle.SetRaw(entity.ID, value);
            }
        }
    }
}