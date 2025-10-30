using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    public interface IOnProvideComponent {
        void OnProvide(GameObject go);
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [DefaultExecutionOrder(short.MaxValue)]
    public partial class StaticEcsEntityProvider : AbstractStaticEcsProvider, IStaticEcsEntityProvider {
        public OnDestroyType OnDestroyType = OnDestroyType.None;
        public bool DisableEntityOnCreate = false;
        public bool SyncOnEnableAndDisable = true;
        public StaticEcsEntityProvider Prefab;
        
        [SerializeReference, HideInInspector] private List<IComponent> components = new();

        [SerializeReference, HideInInspector] private List<ITag> tags = new();

        public IEntity Entity {
            get => _entity;
            set {
                _entity = value;
                if (value != null) {
                    EntityGid = value.Gid();
                }
            }
        }
        internal IEntity _entity;
        
        [HideInInspector] public EntityGID EntityGid;

        protected virtual void Awake() {
            if (UsageType == UsageType.OnAwake) {
                CreateEntity();
                Prefab = null;
            }
        }

        protected virtual void Start() {
            if (UsageType == UsageType.OnStart) {
                CreateEntity();
                Prefab = null;
            }
        }

        protected virtual void OnEnable() {
            if (SyncOnEnableAndDisable && World?.Status() == WorldStatus.Initialized && (Entity?.IsNotDestroyed() ?? false)) {
                Entity.Enable();
            }
        }

        protected virtual void OnDisable() {
            if (SyncOnEnableAndDisable && World?.Status() == WorldStatus.Initialized && (Entity?.IsNotDestroyed() ?? false)) {
                Entity.Disable();
            }
        }

        public bool CreateEntity(bool onCreateEntity = true) {
            if (Prefab) {
                if (Prefab.CreateEntity(onCreateEntity)) {
                    EntityGid = Prefab.EntityGid;
                    _entity = Prefab._entity;
                    _world = Prefab._world;
                    _worldTypeName = Prefab._worldTypeName;
                    Prefab.EntityGid = default;
                    Prefab._entity = default;
                    Prefab._world = default;
                    return true;
                }
                return false;
            }
            
            #if DEBUG
            if (components == null || components.Count == 0) {
                return false;
            }
            #endif
            var value = components[0];
            if (value is IOnProvideComponent provideComponent) {
                provideComponent.OnProvide(gameObject);
            }

            if (World == null) {
                Debug.LogWarning($"You're trying to create an entity in an uninitialized world {WorldEditorName}");
                return false;
            }
            
            Entity = World.NewEntity(value);
            EntityGid = Entity.Gid();
            
            for (var i = 1; i < components.Count; i++) {
                value = components[i];
                #if DEBUG
                if (value == null) {
                    throw new StaticEcsException("[StaticEcsEntityProvider] NULL component");
                }
                #endif
                if (value is IOnProvideComponent e) {
                    e.OnProvide(gameObject);
                }

                Entity.PutRaw(value);
            }

            if (tags != null) {
                foreach (var t in tags) {
                    #if DEBUG
                    if (t == null) {
                        throw new StaticEcsException("[StaticEcsEntityProvider] NULL tag");
                    }
                    #endif
                    Entity.SetTag(t.GetType());
                }
            }

            if (DisableEntityOnCreate) {
                Entity.Disable();
            }

            if (onCreateEntity) {
                OnCreate();
            }

            return true;
        }
        
        
        protected virtual void OnDestroy() {
            if (OnDestroyType == OnDestroyType.DestroyEntity) {
                _entity?.TryDestroy();
            }

            _entity = null;
            EntityGid = default;
        }
    }
        
    public enum OnDestroyType {
        None,
        DestroyEntity
    }
}