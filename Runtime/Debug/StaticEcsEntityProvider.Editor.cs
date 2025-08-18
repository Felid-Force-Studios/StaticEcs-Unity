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

        #if !FFS_ECS_DISABLE_MASKS
        public void Masks(List<IMask> result) {
            if (EntityIsActual()) {
                Entity.GetAllMasks(result);
            } else {
                result.AddRange(masks);
            }
        }
        
        public bool ShouldShowMask(Type maskType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World.TryGetMasksRawPool(maskType, out var _);
        }
        
        public virtual void OnSelectMask(Type maskType) {
            if (EntityIsActual()) {
                Entity.SetMaskByType(maskType);
            } else {
                foreach (var val in masks) {
                    if (val.GetType() == maskType) {
                        return;
                    }
                }

                masks.Add((IMask) Activator.CreateInstance(maskType, true));
            }
        }
        
        public virtual void OnDeleteMask(Type maskType) {
            if (EntityIsActual()) {
                Entity.DeleteMaskByType(maskType);
            } else {
                masks.RemoveAll(mask => mask.GetType() == maskType);
            }
        }

        public void DeleteAllBrokenMasks() {
            masks.RemoveAll(val => val == null);
        }
        #endif
        
        public bool EntityIsActual() {
            return Entity != null && Entity.IsActual() && Entity.Gid() == EntityGid;
        }
        
        public bool HasStandardComponents() {
            if (EntityIsActual()) {
                return Entity.StandardComponentsCount() > 0;
            }

            return standardComponents.Count > 0;
        }
        
        public void StandardComponents(List<IStandardComponent> result) {
            if (EntityIsActual()) {
                Entity.GetAllStandardComponents(result);
            } else {
                result.AddRange(standardComponents);
            }
        }
        
        public bool ShouldShowStandardComponent(Type componentType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World.TryGetStandardComponentsRawPool(componentType, out var _);
        }
        
        public virtual void OnChangeStandardComponent(IStandardComponent component, Type componentType) {
            if (EntityIsActual()) {
                Entity.SetRawStandard(component);
            } else {
                for (var i = 0; i < standardComponents.Count; i++) {
                    var val = standardComponents[i];
                    if (val.GetType() == componentType) {
                        standardComponents[i] = component;
                        return;
                    }
                }
                standardComponents.Add(component);
            }
        }
        
        public virtual void OnSelectStandardComponent(IStandardComponent component) {
            if (EntityIsActual()) {
                Entity.SetRawStandard(component);
            } else {
                for (var i = 0; i < standardComponents.Count; i++) {
                    var val = standardComponents[i];
                    if (val.GetType() == component.GetType()) {
                        standardComponents[i] = component;
                        return;
                    }
                }
                standardComponents.Add(component);
            }
        }
        
        public virtual void OnDeleteStandardComponent(Type componentType) {
            if (!EntityIsActual()) {
                standardComponents.RemoveAll(component => component.GetType() == componentType);
            }
        }

        public void DeleteAllBrokenStandardComponents() {
            standardComponents.RemoveAll(val => val == null);
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
            #if !FFS_ECS_DISABLE_MASKS
            masks.Clear();
            #endif
        }
    }
}
#endif