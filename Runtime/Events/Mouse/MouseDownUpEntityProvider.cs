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
    public abstract class MouseDownUpEntityProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendDownEvent() {
            World<TWorld>.SendEvent(new MouseDownEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendUpEvent() {
            World<TWorld>.SendEvent(new MouseUpEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent() {
            SetComponentOnEntity(new MousePressedState());
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteComponentFromEntity<MousePressedState>();
        }

        private void OnMouseDown() {
            if (!CanSend()) return;
            if (SendEvents) OnSendDownEvent();
            if (ManageComponents) OnAddComponent();
        }

        private void OnMouseUp() {
            if (!CanSend()) return;
            if (SendEvents) OnSendUpEvent();
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class MouseDownUpEntityGIDProvider<TWorld> : MouseDownUpEntityProvider<TWorld>
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
    public abstract class MouseDownUpEntityRefProvider<TWorld, TProvider> : MouseDownUpEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField]
        private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;
    }
}