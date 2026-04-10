using System.Runtime.CompilerServices;
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
    public abstract class DragProvider<TWorld> : GUIEventProvider<TWorld>,
                                                 IBeginDragHandler, IDragHandler, IEndDragHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendStartEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragStartEvent {
                Ref = gameObject,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendMoveEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragMoveEvent {
                Ref = gameObject,
                Position = data.position,
                PointerId = data.pointerId,
                Delta = data.delta,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEndEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new DragEndEvent {
                Ref = gameObject,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        public void OnBeginDrag(PointerEventData data) {
            if (!CanSend()) return;
            OnSendStartEvent(data);
        }

        public void OnDrag(PointerEventData data) {
            if (!CanSend()) return;
            OnSendMoveEvent(data);
        }

        public void OnEndDrag(PointerEventData data) {
            if (!CanSend()) return;
            OnSendEndEvent(data);
        }
    }
}