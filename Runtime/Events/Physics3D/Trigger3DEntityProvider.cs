#if FFS_ECS_PHYSICS
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
    public abstract class Trigger3DEntityProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Collider data) {
            World<TWorld>.SendEvent(new TriggerEnter3DEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Collider = data,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Collider data) {
            World<TWorld>.SendEvent(new TriggerExit3DEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Collider = data,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent(Collider data) {
            SetComponentOnEntity(new Trigger3DState {
                Collider = data,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteComponentFromEntity<Trigger3DState>();
        }

        private void OnTriggerEnter(Collider data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEnterEvent(data);
            if (ManageComponents) OnAddComponent(data);
        }

        private void OnTriggerExit(Collider data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendExitEvent(data);
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class Trigger3DEntityGIDProvider<TWorld> : Trigger3DEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [UnityEngine.SerializeField]
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
    public abstract class Trigger3DEntityRefProvider<TWorld, TProvider> : Trigger3DEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [UnityEngine.SerializeField]
        private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;

        #if UNITY_EDITOR
        protected void Reset() {
            if (entityProvider == null) entityProvider = GetComponent<TProvider>();
        }
        #endif
    }
}
#endif