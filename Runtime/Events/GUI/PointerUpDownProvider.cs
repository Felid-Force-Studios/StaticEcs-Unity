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
    public abstract class PointerUpDownProvider<TWorld> : GUIEventProvider<TWorld>,
        IPointerDownHandler, IPointerUpHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendDownEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerDownEvent {
                Ref = gameObject,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendUpEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerUpEvent {
                Ref = gameObject,
                Position = data.position,
                PointerId = data.pointerId,
                Button = data.button,
            });
        }

        public void OnPointerDown(PointerEventData data) {
            if (!CanSend()) return;
            OnSendDownEvent(data);
        }

        public void OnPointerUp(PointerEventData data) {
            if (!CanSend()) return;
            OnSendUpEvent(data);
        }
    }
}
