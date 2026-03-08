using System.Collections.Generic;
using UnityEngine;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    
    public interface IOnProvideComponent {
        void OnProvide(GameObject go);
    }

    public enum OnDestroyType {
        None,
        DestroyEntity
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public abstract partial class StaticEcsEntityProvider<TWorld> : AbstractStaticEcsProvider
        where TWorld : struct, IWorldType {
        
        public OnDestroyType onDestroyType = OnDestroyType.None;
        public bool disableEntityOnCreate;
        public bool onEnableAndDisable = true;
        public AbstractStaticEcsProvider prefab;
        
        [SerializeReference, HideInInspector] protected List<IComponent> components = new();
        [SerializeReference, HideInInspector] protected List<ITag> tags = new();
        [HideInInspector] public byte entityType;
        [HideInInspector] public EntityGID entityGid;
        
        public EntityGID EntityGid {
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

        public virtual bool CreateEntity(bool onCreateEntity = true) {
            if (prefab && prefab is StaticEcsEntityProvider<TWorld> prefabProvider) {
                if (prefabProvider.CreateEntity(onCreateEntity)) {
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

            ref var world = ref World<TWorld>.Data.Handle;
            var entity = World<TWorld>.NewEntity(entityType);

            entityGid = entity;

            var eid = entity.ID;
            if (components != null) {
                for (var i = 0; i < components.Count; i++) {
                    var value = components[i];
                    if (value != null) {
                        if (value is IOnProvideComponent e) {
                            e.OnProvide(gameObject);
                        }

                        if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                            handle.SetRaw(eid, value);
                        }
                    }
                    #if DEBUG
                    else {
                        throw new StaticEcsException("[StaticEcsEntityProvider] NULL component");
                    }
                    #endif
                }
            }

            if (tags != null) {
                foreach (var value in tags) {
                    if (value != null) {
                        if (world.TryGetComponentsHandle(value.GetType(), out var handle)) {
                            handle.Set(eid);
                        }
                    }
                    #if DEBUG
                    else {
                        throw new StaticEcsException("[StaticEcsEntityProvider] NULL tag");
                    }
                    #endif
                }
            }

            if (disableEntityOnCreate) {
                entityGid.Unpack<TWorld>().Disable();
            }

            if (onCreateEntity) {
                OnCreate();
            }

            return true;
        }
    }
}
