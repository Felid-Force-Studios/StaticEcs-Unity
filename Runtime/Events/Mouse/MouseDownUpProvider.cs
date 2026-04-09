using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class MouseDownUpProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendDownEvent() {
            World<TWorld>.SendEvent(new MouseDownEvent {
                Ref = gameObject,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendUpEvent() {
            World<TWorld>.SendEvent(new MouseUpEvent {
                Ref = gameObject,
            });
        }

        private void OnMouseDown() {
            if (!CanSend()) return;
            OnSendDownEvent();
        }

        private void OnMouseUp() {
            if (!CanSend()) return;
            OnSendUpEvent();
        }
    }
}