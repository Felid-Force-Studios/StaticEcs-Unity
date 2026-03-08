#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    
    public abstract partial class StaticEcsEntityProvider<TWorld> {
        private void OnValidate() {
            if (prefab && prefab.gameObject.scene.IsValid()) {
                Debug.LogWarning($"The {nameof(prefab)} field should reference to the prefab, not to an object in the scene.", this);
                prefab = null;
            }

            if (prefab && !gameObject.scene.IsValid()) {
                prefab = null;
            }
        }
        
        public virtual bool EntityIsActual() {
            return World<TWorld>.Status == WorldStatus.Initialized
                   && entityGid.Status<TWorld>() == GIDStatus.Active;
        }

        public virtual bool IsPrefab() {
            return !gameObject.scene.IsValid();
        }

        public virtual bool HasComponents() {
            return components.Count > 0;
        }

        public virtual bool IsDisabled(Type componentType) {
            if (!EntityIsActual()) return false;
            return World<TWorld>.Data.Handle.TryGetComponentsHandle(componentType, out var handle) && handle.HasDisabled(EntityGid.Id);
        }

        public virtual void Disable(Type componentType) {
            if (!EntityIsActual()) return;
            EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                Type = DebugCommandType.DisableComponent,
                EntityGid = EntityGid,
                TargetType = componentType,
            });
        }

        public virtual void Enable(Type componentType) {
            if (!EntityIsActual()) return;
            EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                Type = DebugCommandType.EnableComponent,
                EntityGid = EntityGid,
                TargetType = componentType,
            });
        }

        public virtual void Components(List<IComponent> result) {
            if (EntityIsActual()) {
                var eid = EntityGid.Id;
                foreach (var compHandle in World<TWorld>.Data.Handle.GetAllComponentsHandles()) {
                    if (!compHandle.IsTag && compHandle.TryGetRaw(eid, out var comp)) {
                        result.Add((IComponent)comp);
                    }
                }
            } else {
                result.AddRange(components);
            }
        }

        public virtual bool ShouldShowComponent(Type componentType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World<TWorld>.IsWorldInitialized
                   && World<TWorld>.Data.Handle.TryGetComponentsHandle(componentType, out _);
        }

        public virtual void OnSelectComponent(IComponent component) {
            if (EntityIsActual()) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = EntityGid,
                    TargetType = component.GetType(),
                    Value = component,
                });
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

        public virtual void OnChangeComponent(IComponent component, Type componentType) {
            if (EntityIsActual()) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetComponent,
                    EntityGid = EntityGid,
                    TargetType = componentType,
                    Value = component,
                });
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

        public virtual void OnDeleteComponent(Type componentType) {
            if (EntityIsActual()) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.Delete,
                    EntityGid = EntityGid,
                    TargetType = componentType,
                });
            } else {
                components.RemoveAll(component => component.GetType() == componentType);
            }
        }

        public virtual void DeleteAllBrokenComponents() {
            components.RemoveAll(val => val == null);
        }

        public virtual void Tags(List<ITag> result) {
            if (EntityIsActual()) {
                var eid = EntityGid.Id;
                foreach (var tagHandle in World<TWorld>.Data.Handle.GetAllComponentsHandles()) {
                    if (tagHandle.IsTag && tagHandle.Has(eid)) {
                        result.Add((ITag)tagHandle.DefaultValue());
                    }
                }
            } else {
                result.AddRange(tags);
            }
        }

        public virtual bool ShouldShowTag(Type tagType, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World<TWorld>.Status == WorldStatus.Initialized
                   && World<TWorld>.Data.Handle.TryGetComponentsHandle(tagType, out _);
        }

        public virtual void OnSelectTag(Type tagType) {
            if (EntityIsActual()) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.SetTag,
                    EntityGid = EntityGid,
                    TargetType = tagType,
                });
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
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.Delete,
                    EntityGid = EntityGid,
                    TargetType = tagType,
                });
            } else {
                tags.RemoveAll(val => val.GetType() == tagType);
            }
        }

        public virtual void DeleteAllBrokenTags() {
            tags.RemoveAll(val => val == null);
        }

        public virtual void Clear() {
            components.Clear();
            tags.Clear();
            entityType = 0;
        }

        public AbstractStaticEcsProvider GetPrefab() => prefab;
        
        public void ClearPrefab() => prefab = null;
    }
}
#endif
