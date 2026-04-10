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
    public abstract class PointerUpDownEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>,
                                                                IPointerDownHandler, IPointerUpHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendDownEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerDownEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendUpEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerUpEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent(PointerEventData data) {
            SetTagOnEntity<PointerPressedState>();
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteTagFromEntity<PointerPressedState>();
        }

        public void OnPointerDown(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendDownEvent(data);
            if (ManageComponents) OnAddComponent(data);
        }

        public void OnPointerUp(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendUpEvent(data);
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class PointerUpDownEntityGIDProvider<TWorld> : PointerUpDownEntityProvider<TWorld>
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
    public abstract class PointerUpDownEntityRefProvider<TWorld, TProvider> : PointerUpDownEntityProvider<TWorld>
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