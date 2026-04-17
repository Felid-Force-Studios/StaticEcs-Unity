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
            return providers.Count > 0;
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

        private static readonly List<ComponentProvider> _componentProviderPool = new();
        private static readonly List<TagProvider> _tagProviderPool = new();
        private static int _componentPoolIdx;
        private static int _tagPoolIdx;

        private static ComponentProvider RentComponentProvider(IComponent value) {
            ComponentProvider p;
            if (_componentPoolIdx < _componentProviderPool.Count) {
                p = _componentProviderPool[_componentPoolIdx];
            } else {
                p = new ComponentProvider();
                _componentProviderPool.Add(p);
            }

            _componentPoolIdx++;
            p.value = value;
            return p;
        }

        private static TagProvider RentTagProvider(ITag value) {
            TagProvider p;
            if (_tagPoolIdx < _tagProviderPool.Count) {
                p = _tagProviderPool[_tagPoolIdx];
            } else {
                p = new TagProvider();
                _tagProviderPool.Add(p);
            }

            _tagPoolIdx++;
            p.value = value;
            return p;
        }

        public static void ResetProviderPool() {
            _componentPoolIdx = 0;
            _tagPoolIdx = 0;
        }

        public virtual void GetProviders(List<IComponentOrTagProvider> result) {
            if (EntityIsActual()) {
                ResetProviderPool();
                var eid = EntityGid.Id;
                foreach (var handle in World<TWorld>.Data.Handle.GetAllComponentsHandles()) {
                    if (handle.IsTag) {
                        if (handle.Has(eid)) {
                            result.Add(RentTagProvider((ITag) handle.DefaultValue()));
                        }
                    } else {
                        if (handle.TryGetRaw(eid, out var comp)) {
                            result.Add(RentComponentProvider((IComponent) comp));
                        }
                    }
                }
            } else {
                result.AddRange(providers);
            }
        }

        public virtual bool ShouldShowProvider(Type type, bool runtime) {
            if (!EntityIsActual() && !runtime) return true;
            return World<TWorld>.IsWorldInitialized
                   && World<TWorld>.Data.Handle.TryGetComponentsHandle(type, out _);
        }

        public virtual void OnSelectProvider(IComponentOrTagProvider provider) {
            if (EntityIsActual()) {
                provider.Apply(Entity, true);
            } else {
                for (var i = 0; i < providers.Count; i++) {
                    if (providers[i].ComponentType == provider.ComponentType) {
                        providers[i] = provider;
                        return;
                    }
                }

                providers.Add(provider);
            }
        }

        public virtual void OnChangeProvider(IComponentOrTagProvider provider, Type componentType, bool deferred = true) {
            if (EntityIsActual()) {
                provider.Apply(Entity, deferred);
            } else {
                for (var i = 0; i < providers.Count; i++) {
                    if (providers[i].ComponentType == componentType) {
                        providers[i] = provider;
                        return;
                    }
                }

                providers.Add(provider);
            }
        }

        public virtual void OnDeleteProvider(Type type) {
            if (EntityIsActual()) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.Delete,
                    EntityGid = EntityGid,
                    TargetType = type,
                });
            } else {
                providers.RemoveAll(p => p.ComponentType == type);
            }
        }

        public virtual void DeleteAllBrokenProviders() {
            providers.RemoveAll(val => val == null || val.ComponentType == null);
        }

        public virtual void Clear() {
            providers.Clear();
            entityType = 0;
        }

        public List<IComponentOrTagProvider> SerializedProviders => providers;

        public AbstractStaticEcsProvider GetPrefab() => prefab;

        public void ClearPrefab() => prefab = null;
    }
}
#endif