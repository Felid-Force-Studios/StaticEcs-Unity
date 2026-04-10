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
    public abstract class SubmitCancelProvider<TWorld> : GUIEventProvider<TWorld>, ISubmitHandler, ICancelHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendSubmitEvent() {
            World<TWorld>.SendEvent(new SubmitEvent {
                Ref = gameObject,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendCancelEvent() {
            World<TWorld>.SendEvent(new CancelEvent {
                Ref = gameObject,
            });
        }

        public void OnSubmit(BaseEventData eventData) {
            if (!CanSend()) return;
            OnSendSubmitEvent();
        }

        public void OnCancel(BaseEventData eventData) {
            if (!CanSend()) return;
            OnSendCancelEvent();
        }
    }
}