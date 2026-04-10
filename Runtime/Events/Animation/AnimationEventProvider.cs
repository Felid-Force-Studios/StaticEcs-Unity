#if FFS_ECS_ANIMATION
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
    public abstract class AnimationEventProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(AnimationEvent evt) {
            World<TWorld>.SendEvent(new AnimationEventEcsEvent {
                Ref = gameObject,
                StringParameter = evt.stringParameter,
                IntParameter = evt.intParameter,
                FloatParameter = evt.floatParameter,
                ObjectReferenceParameter = evt.objectReferenceParameter,
                AnimationState = evt.animationState,
            });
        }

        public void OnAnimationEvent(AnimationEvent evt) {
            if (!CanSend()) return;
            OnSendEvent(evt);
        }
    }
}
#endif