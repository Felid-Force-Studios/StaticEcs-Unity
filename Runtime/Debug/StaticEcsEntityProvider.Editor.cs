#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    public partial class StaticEcsEntityProvider {
        
        private void OnValidate() {
            if (Prefab && Prefab.gameObject.scene.IsValid()) {
                Debug.LogWarning($"The {nameof(Prefab)} field should reference to the prefab, not to an object in the scene.", this);
                Prefab = null;
            }

            if (Prefab && !gameObject.scene.IsValid()) {
                Prefab = null;
            }
        }
        
        public bool IsPrefab() {
            return !gameObject.scene.IsValid();
        }

        #if !FFS_ECS_DISABLE_TAGS
        public void Tags(List<ITag> result) {
            if (EntityIsActual()) {
                Entity.GetAllTags(result);
            } else {
                result.AddRange(tags);
            }
        }
        
        public bool ShouldShowTag(Type tagType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World.TryGetTagsRawPool(tagType, out var _);
        }
        
        public virtual void OnSelectTag(Type tagType) {
            if (EntityIsActual()) {
                Entity.SetTag(tagType);
            } else {
                foreach (var val in tags) {
                    if (val.GetType() == tagType) {
                        return;
                    }
                }

                tags.Add((ITag) Activator.CreateInstance(tagType, true));
            }
        }
        
        public virtual void OnDeleteTag(Type tagType) {
            if (EntityIsActual()) {
                Entity.DeleteTag(tagType);
            } else {
                tags.RemoveAll(tag => tag.GetType() == tagType);
            }
        }
        
        public void DeleteAllBrokenTags() {
            tags.RemoveAll(val => val == null);
        }
        #endif
        
        public bool EntityIsActual() {
            return Entity != null && Entity.IsActual() && Entity.Gid() == EntityGid;
        }

        public bool HasComponents() {
            return components.Count > 0;
        }

        public bool IsDisabled(Type componentType) {
            if (!EntityIsActual()) return false;
            return World.TryGetComponentsRawPool(componentType, out var pool) && pool.HasDisabled(Entity.GetId());
        }

        public void Disable(Type componentType) {
            if (!EntityIsActual()) return;

            if (World.TryGetComponentsRawPool(componentType, out var pool)) {
                pool.Disable(Entity.GetId());
            }
        }

        public void Enable(Type componentType) {
            if (!EntityIsActual()) return;

            if (World.TryGetComponentsRawPool(componentType, out var pool)) {
                pool.Enable(Entity.GetId());
            }
        }

        public void Components(List<IComponent> result) {
            if (EntityIsActual()) {
                Entity.GetAllComponents(result);
            } else {
                result.AddRange(components);
            }
        }

        public bool ShouldShowComponent(Type componentType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World.TryGetComponentsRawPool(componentType, out var _);
        }

        public virtual void OnChangeComponent(IComponent component, Type componentType) {
            if (EntityIsActual()) {
                Entity.PutRaw(component);
            } else {
                for (var i = 0; i < components.Count; i++) {
                    var val = components[i];
                    if (val.GetType() == componentType) {
                        components[i] = component;
                        return;
                    }
                }
                components.Add(component);
            }
        }

        public virtual void OnSelectComponent(IComponent component) {
            if (EntityIsActual()) {
                Entity.PutRaw(component);
            } else {
                for (var i = 0; i < components.Count; i++) {
                    var val = components[i];
                    if (val.GetType() == component.GetType()) {
                        components[i] = component;
                        return;
                    }
                }
                components.Add(component);
            }
        }

        public virtual void OnDeleteComponent(Type componentType) {
            if (EntityIsActual()) {
                Entity.Delete(componentType);
            } else {
                components.RemoveAll(component => component.GetType() == componentType);
            }
        }

        public void DeleteAllBrokenComponents() {
            components.RemoveAll(val => val == null);
        }

        public void Clear() {
            components.Clear();
            #if !FFS_ECS_DISABLE_TAGS
            tags.Clear();
            #endif
        }
    }
}
#endif