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
    public abstract class DragEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>,
        IBeginDragHandler, IDragHandler, IEndDragHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendStartEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragStartEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendMoveEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragMoveEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                PointerId = data.pointerId,
                Delta = data.delta,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEndEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragEndEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnAddComponent(PointerEventData data) {
            SetComponentOnEntity(new DragState {
                Position = data.position,
                PointerId = data.pointerId,
                Delta = data.delta,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnUpdateComponent(PointerEventData data) {
            SetComponentOnEntity(new DragState {
                Position = data.position,
                PointerId = data.pointerId,
                Delta = data.delta,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnRemoveComponent() {
            DeleteComponentFromEntity<DragState>();
        }

        public void OnBeginDrag(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendStartEvent(data);
            if (ManageComponents) OnAddComponent(data);
        }

        public void OnDrag(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendMoveEvent(data);
            if (ManageComponents) OnUpdateComponent(data);
        }

        public void OnEndDrag(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEndEvent(data);
            if (ManageComponents) OnRemoveComponent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class DragEntityGIDProvider<TWorld> : DragEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField] private EntityGID entityGid;
        protected override EntityGID EntityGID { [MethodImpl(AggressiveInlining)] get => entityGid; }
        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class DragEntityRefProvider<TWorld, TProvider> : DragEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField] private TProvider entityProvider;
        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityProvider != null ? entityProvider.EntityGid : default;
        }
        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;
    }
}
