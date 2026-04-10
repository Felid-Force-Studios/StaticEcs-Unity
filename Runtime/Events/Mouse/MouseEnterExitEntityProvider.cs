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
    public abstract class MouseEnterExitEntityProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent() {
            World<TWorld>.SendEvent(new MouseEnterEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent() {
            World<TWorld>.SendEvent(new MouseExitEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent() {
            SetTagOnEntity<MouseHoverState>();
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteTagFromEntity<MouseHoverState>();
        }

        private void OnMouseEnter() {
            if (!CanSend()) return;
            if (SendEvents) OnSendEnterEvent();
            if (ManageComponents) OnAddComponent();
        }

        private void OnMouseExit() {
            if (!CanSend()) return;
            if (SendEvents) OnSendExitEvent();
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class MouseEnterExitEntityGIDProvider<TWorld> : MouseEnterExitEntityProvider<TWorld>
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
    public abstract class MouseEnterExitEntityRefProvider<TWorld, TProvider> : MouseEnterExitEntityProvider<TWorld>
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
            if (entityProvider == null) entityProvider = GetComponent<TProvider>();
        }
        #endif
    }
}