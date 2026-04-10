using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class PointerEnterExitEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>,
                                                                   IPointerEnterHandler, IPointerExitHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerEnterEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerExitEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent(PointerEventData data) {
            SetTagOnEntity<PointerHoverState>();
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteTagFromEntity<PointerHoverState>();
        }

        public void OnPointerEnter(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEnterEvent(data);
            if (ManageComponents) OnAddComponent(data);
        }

        public void OnPointerExit(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendExitEvent(data);
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class PointerEnterExitEntityGIDProvider<TWorld> : PointerEnterExitEntityProvider<TWorld>
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
    public abstract class PointerEnterExitEntityRefProvider<TWorld, TProvider> : PointerEnterExitEntityProvider<TWorld>
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