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
    public abstract class StaticEcsStateMachineBehaviourLinker<TWorld, TProvider> : MonoBehaviour
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField]
        private TProvider entityProvider;
        [SerializeField]
        private Animator animator;

        void Start() {
            Link();
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;

        [MethodImpl(AggressiveInlining)]
        public void SetAnimator(Animator value) => animator = value;

        #if UNITY_EDITOR
        void Reset() {
            if (entityProvider == null) entityProvider = GetComponent<TProvider>();
            if (animator == null) animator = GetComponent<Animator>();
        }
        #endif

        public void Link() {
            if (animator == null) animator = GetComponent<Animator>();
            if (entityProvider == null || animator == null) return;
            var behaviours = animator.GetBehaviours<StaticEcsEntityStateMachineBehaviour<TWorld>>();
            var gid = entityProvider.EntityGid;
            for (var i = 0; i < behaviours.Length; i++) {
                behaviours[i].SetEntityGID(gid);
            }
        }
    }
}
#endif