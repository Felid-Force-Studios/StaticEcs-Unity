#if FFS_ECS_PHYSICS
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    [DefaultExecutionOrder(short.MaxValue)]
    public abstract class ContactColliderProvider<TWorld> : MonoBehaviour
        where TWorld : struct, IWorldType {

        [SerializeField]
        private bool registerChildColliders;

        private Collider[] _colliders;

        protected abstract EntityGID EntityGID { get; }

        private void Awake() {
            Register();
        }

        private void OnDestroy() {
            Unregister();
        }

        [MethodImpl(AggressiveInlining)]
        private void Register() {
            if (World<TWorld>.Status != WorldStatus.Initialized) {
                Debug.LogWarning($"[ContactColliderProvider] You're trying to register colliders in an uninitialized world {typeof(TWorld).Name}");
                return;
            }

            if (!World<TWorld>.HasResource<ContactColliderEntityMap>()) {
                World<TWorld>.SetResource(new ContactColliderEntityMap { Map = new Dictionary<int, EntityGID>() });
            }

            ref var map = ref World<TWorld>.GetResource<ContactColliderEntityMap>();

            _colliders = registerChildColliders
                ? GetComponentsInChildren<Collider>()
                : GetComponents<Collider>();

            var gid = EntityGID;
            for (var i = 0; i < _colliders.Length; i++) {
                _colliders[i].providesContacts = true;
                map.Map[_colliders[i].GetInstanceID()] = gid;
            }
        }

        [MethodImpl(AggressiveInlining)]
        private void Unregister() {
            if (World<TWorld>.Status != WorldStatus.Initialized) return;
            if (!World<TWorld>.HasResource<ContactColliderEntityMap>()) return;
            if (_colliders == null) return;
            ref var map = ref World<TWorld>.GetResource<ContactColliderEntityMap>();

            for (var i = 0; i < _colliders.Length; i++) {
                if (_colliders[i] != null) {
                    map.Map.Remove(_colliders[i].GetInstanceID());
                }
            }

            _colliders = null;
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ContactColliderGIDProvider<TWorld> : ContactColliderProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField]
        private EntityGID entityGid;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityGid;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ContactColliderRefProvider<TWorld, TProvider> : ContactColliderProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField]
        private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;

        #if UNITY_EDITOR
        protected void Reset() {
            if (entityProvider == null) {
                entityProvider = GetComponent<TProvider>();
            }
        }
        #endif
    }
}
#endif