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
        [SerializeReference, HideInInspector] private List<IComponent> components = new();
        [SerializeReference, HideInInspector] private List<IStandardComponent> standardComponents = new();

        #if !FFS_ECS_DISABLE_TAGS
        [SerializeReference, HideInInspector] private List<ITag> tags = new();
        #endif
        
        #if !FFS_ECS_DISABLE_MASKS
        [SerializeReference, HideInInspector] private List<IMask> masks = new();
        #endif

        public IEntity Entity {
            get => _entity;
            set {
                _entity = value;
                if (value != null) {
                    PackedEntity = value.Pack();
                }
            }
        }
        private IEntity _entity;
        
        [HideInInspector] public PackedEntity PackedEntity;

        protected virtual void Start() {
            if (UsageType == UsageType.OnStart) {
                CreateEntity();
            }
        }

        public bool CreateEntity(bool onCreateEntity = true) {
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
            PackedEntity = Entity.Pack();
            
            for (var i = 0; i < standardComponents.Count; i++) {
                var standardComponent = standardComponents[i];
                if (standardComponent is EntityVersion) {
                    continue;
                }
                #if DEBUG
                if (standardComponent == null) {
                    throw new Exception("[StaticEcsEntityProvider] NULL standardComponent");
                }
                #endif
                if (standardComponent is IOnProvideComponent e) {
                    e.OnProvide(gameObject);
                }

                Entity.SetRawStandard(standardComponent);
            }
            
            for (var i = 1; i < components.Count; i++) {
                value = components[i];
                #if DEBUG
                if (value == null) {
                    throw new Exception("[StaticEcsEntityProvider] NULL component");
                }
                #endif
                if (value is IOnProvideComponent e) {
                    e.OnProvide(gameObject);
                }

                Entity.PutRaw(value);
            }

            #if !FFS_ECS_DISABLE_TAGS
            if (tags != null) {
                foreach (var t in tags) {
                    #if DEBUG
                    if (t == null) {
                        throw new Exception("[StaticEcsEntityProvider] NULL tag");
                    }
                    #endif
                    Entity.SetTag(t.GetType());
                }
            }
            #endif

            #if !FFS_ECS_DISABLE_MASKS
            if (masks != null) {
                foreach (var m in masks) {
                    #if DEBUG
                    if (m == null) {
                        throw new Exception("[StaticEcsEntityProvider] NULL mask");
                    }
                    #endif
                    Entity.SetMask(m.GetType());
                }
            }
            #endif

            if (onCreateEntity) {
                OnCreate();
            }

            return true;
        }
    }
}