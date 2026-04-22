using System.Collections.Generic;
using UnityEngine;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    public enum OnDestroyType {
        None, DestroyEntity
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public abstract partial class StaticEcsEntityProvider<TWorld> : AbstractStaticEcsEntityProvider
        where TWorld : struct, IWorldType {

        public OnDestroyType onDestroyType = OnDestroyType.None;
        public bool disableEntityOnCreate;
        public bool onEnableAndDisable = true;
        public AbstractStaticEcsProvider prefab;

        [SerializeReference, HideInInspector]
        protected List<IComponentOrTagProvider> providers = new();
        [HideInInspector]
        public byte entityType;
        [HideInInspector]
        public EntityGID entityGid;

        public override EntityGID EntityGid {
            get => entityGid;
            set => entityGid = value;
        }

        public World<TWorld>.Entity Entity => entityGid.Unpack<TWorld>();

        protected void Awake() {
            if (UsageType == UsageType.OnAwake) {
                CreateEntity();
                prefab = null;
            }
        }

        protected void Start() {
            if (UsageType == UsageType.OnStart) {
                CreateEntity();
                prefab = null;
            }
        }

        protected void OnEnable() {
            if (onEnableAndDisable
                && World<TWorld>.Status == WorldStatus.Initialized
                && entityGid.TryUnpack<TWorld>(out var e)) {
                e.Enable();
            }
        }

        protected void OnDisable() {
            if (onEnableAndDisable
                && World<TWorld>.Status == WorldStatus.Initialized
                && entityGid.TryUnpack<TWorld>(out var e)) {
                e.Disable();
            }
        }

        protected void OnDestroy() {
            if (onDestroyType == OnDestroyType.DestroyEntity
                && World<TWorld>.Status == WorldStatus.Initialized
                && entityGid.Status<TWorld>() == GIDStatus.Active) {
                entityGid.Unpack<TWorld>().Destroy();
            }

            entityGid = default;
        }

        public override bool CreateEntity() {
            if (prefab && prefab is StaticEcsEntityProvider<TWorld> prefabProvider) {
                if (prefabProvider.CreateEntity()) {
                    entityGid = prefabProvider.EntityGid;
                    prefabProvider.EntityGid = default;
                    return true;
                }

                return false;
            }

            if (World<TWorld>.Status != WorldStatus.Initialized) {
                Debug.LogWarning($"You're trying to create an entity in an uninitialized world {typeof(TWorld).Name}");
                return false;
            }

            var entity = World<TWorld>.NewEntity(entityType);
            entityGid = entity;

            if (providers != null) {
                for (var i = 0; i < providers.Count; i++) {
                    var provider = providers[i];
                    if (provider != null) {
                        provider.Apply(entity);
                    }
                    #if DEBUG
                    else {
                        throw new StaticEcsException("[StaticEcsEntityProvider] NULL component or tag");
                    }
                    #endif
                }
            }

            if (disableEntityOnCreate) {
                entityGid.Unpack<TWorld>().Disable();
            }

            PostEntityProviderCreate.Instance.Register(this);
            return true;
        }

        public override void ResolveLinks() {
            if (providers == null) return;
            if (World<TWorld>.Status != WorldStatus.Initialized) return;
            if (entityGid.Status<TWorld>() != GIDStatus.Active) return;

            var entity = entityGid.Unpack<TWorld>();
            ref var world = ref World<TWorld>.Data.Handle;

            for (var i = 0; i < providers.Count; i++) {
                var provider = providers[i];

                if (provider is LinkProvider { target: not null } lp) {
                    lp.value.SetValue(lp.target.EntityGid);
                    if (world.TryGetComponentsHandle(lp.value.GetType(), out var handle)) {
                        handle.SetRaw(entity.ID, (IComponentOrTag) lp.value);
                    }
                } else if (provider is LinksProvider { targets: not null } lsp && lsp.targets.Count > 0 && lsp.value != null) {
                    if (world.TryGetComponentsHandle(lsp.value.GetType(), out var handle)) {
                        handle.SetRaw(entity.ID, (IComponentOrTag) lsp.value);
                        var hasRaw = handle.TryGetRaw(entity.ID, out var raw);
                        if (hasRaw && raw is ILinksComponent linksComponent) {
                            for (var j = 0; j < lsp.targets.Count; j++) {
                                var t = lsp.targets[j];
                                if (t != null) {
                                    linksComponent.AddLink(t.EntityGid);
                                }
                            }

                            handle.SetRawDirect(entity.ID, raw);
                        }
                    }
                }
            }
        }
    }
}