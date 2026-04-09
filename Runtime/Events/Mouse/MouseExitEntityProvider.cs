using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity
{
#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
#endif
    public abstract class MouseExitEntityProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnMouseExitEvent() {
            World<TWorld>.SendEvent(new MouseExitEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        private void OnMouseExit() {
            if (!CanSend()) return;
            OnMouseExitEvent();
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
#endif
    public abstract class MouseExitEntityGIDProvider<TWorld> : MouseExitEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [UnityEngine.SerializeField] private EntityGID entityGid;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityGid;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }
}
