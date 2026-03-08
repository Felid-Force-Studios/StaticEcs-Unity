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
    public abstract class Collision2DProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Collision2D data) {
            var cp = data.GetContact(0);
            World<TWorld>.SendEvent(new CollisionEnter2DEvent {
                Ref = gameObject,
                Collider = data.collider,
                Velocity = data.relativeVelocity,
                Point = cp.point,
                Normal = cp.normal,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Collision2D data) {
            World<TWorld>.SendEvent(new CollisionExit2DEvent {
                Ref = gameObject,
                Collider = data.collider,
                Velocity = data.relativeVelocity,
            });
        }

        private void OnCollisionEnter2D(Collision2D data) {
            if (!CanSend()) return;
            OnSendEnterEvent(data);
        }

        private void OnCollisionExit2D(Collision2D data) {
            if (!CanSend()) return;
            OnSendExitEvent(data);
        }
    }
}
#endif
