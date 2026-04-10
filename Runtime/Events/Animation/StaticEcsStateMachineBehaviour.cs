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
    public abstract class StaticEcsStateMachineBehaviour<TWorld> : StateMachineBehaviour
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual bool CanSend() => World<TWorld>.Status == WorldStatus.Initialized;

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            World<TWorld>.SendEvent(new AnimatorStateEnterEvent {
                Ref = animator.gameObject,
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            World<TWorld>.SendEvent(new AnimatorStateExitEvent {
                Ref = animator.gameObject,
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
            });
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!CanSend()) return;
            OnSendEnterEvent(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!CanSend()) return;
            OnSendExitEvent(animator, stateInfo, layerIndex);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class StaticEcsEntityStateMachineBehaviour<TWorld> : StateMachineBehaviour
        where TWorld : struct, IWorldType {

        [SerializeField]
        private EntityGID entityGid;

        [MethodImpl(AggressiveInlining)]
        protected virtual bool CanSend() => World<TWorld>.Status == WorldStatus.Initialized;

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEnterEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            World<TWorld>.SendEvent(new AnimatorStateEnterEntityEvent {
                Ref = animator.gameObject,
                EntityGID = entityGid,
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            World<TWorld>.SendEvent(new AnimatorStateExitEntityEvent {
                Ref = animator.gameObject,
                EntityGID = entityGid,
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
            });
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!CanSend()) return;
            OnSendEnterEvent(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!CanSend()) return;
            OnSendExitEvent(animator, stateInfo, layerIndex);
        }
    }
}
#endif