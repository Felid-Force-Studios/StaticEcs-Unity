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
    public abstract class PointerEnterExitProvider<TWorld> : GUIEventProvider<TWorld>,
                                                             IPointerEnterHandler, IPointerExitHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerEnterEvent {
                Ref = gameObject,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new PointerExitEvent {
                Ref = gameObject,
            });
        }

        public void OnPointerEnter(PointerEventData data) {
            if (!CanSend()) return;
            OnSendEnterEvent(data);
        }

        public void OnPointerExit(PointerEventData data) {
            if (!CanSend()) return;
            OnSendExitEvent(data);
        }
    }
}