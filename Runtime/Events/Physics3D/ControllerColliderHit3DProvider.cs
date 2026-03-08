#if FFS_ECS_PHYSICS
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
    public abstract class ControllerColliderHit3DProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(ControllerColliderHit data) {
            World<TWorld>.SendEvent(new ControllerColliderHit3DEvent {
                Ref = gameObject,
                Collider = data.collider,
                Point = data.point,
                Normal = data.normal,
                MoveDirection = data.moveDirection,
            });
        }

        private void OnControllerColliderHit(ControllerColliderHit data) {
            if (!CanSend()) return;
            OnSendEvent(data);
        }
    }
}
#endif
