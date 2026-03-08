#if FFS_ECS_PHYSICS2D
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
    public abstract class Trigger2DProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Collider2D data) {
            World<TWorld>.SendEvent(new TriggerEnter2DEvent {
                Ref = gameObject,
                Collider = data,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Collider2D data) {
            World<TWorld>.SendEvent(new TriggerExit2DEvent {
                Ref = gameObject,
                Collider = data,
            });
        }

        private void OnTriggerEnter2D(Collider2D data) {
            if (!CanSend()) return;
            OnSendEnterEvent(data);
        }

        private void OnTriggerExit2D(Collider2D data) {
            if (!CanSend()) return;
            OnSendExitEvent(data);
        }
    }
}
#endif
