#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    public partial class StaticEcsEntityProvider {
        public bool HasComponents() {
            return components.Count > 0;
        }

        public void Components(List<IComponent> result) {
            if (EntityIsActual()) {
                Entity.GetAllComponents(result);
            } else {
                result.AddRange(components);
            }
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
                Entity.SetMask(maskType);
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
                Entity.DeleteMask(maskType);
            } else {
                masks.RemoveAll(mask => mask.GetType() == maskType);
            }
        }
        #endif
        
        public bool EntityIsActual() {
            return Entity != null && Entity.Version() == PackedEntity._version && Entity.IsActual();
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