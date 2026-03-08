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
    public abstract class Collision3DEntityProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Collision data) {
            var cp = data.GetContact(0);
            World<TWorld>.SendEvent(new CollisionEnter3DEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Collider = data.collider,
                Velocity = data.relativeVelocity,
                Point = cp.point,
                Normal = cp.normal,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Collision data) {
            World<TWorld>.SendEvent(new CollisionExit3DEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Collider = data.collider,
                Velocity = data.relativeVelocity,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent(Collision data) {
            var cp = data.GetContact(0);
            SetComponentOnEntity(new Collision3DState {
                Collider = data.collider,
                Velocity = data.relativeVelocity,
                Point = cp.point,
                Normal = cp.normal,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteComponentFromEntity<Collision3DState>();
        }

        private void OnCollisionEnter(Collision data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEnterEvent(data);
            if (ManageComponents) OnAddComponent(data);
        }

        private void OnCollisionExit(Collision data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendExitEvent(data);
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class Collision3DEntityGIDProvider<TWorld> : Collision3DEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [UnityEngine.SerializeField] private EntityGID entityGid;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityGid;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class Collision3DEntityRefProvider<TWorld, TProvider> : Collision3DEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [UnityEngine.SerializeField] private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;
    }
}
#endif
