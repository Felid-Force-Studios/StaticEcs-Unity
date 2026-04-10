using System.Runtime.CompilerServices;
using UnityEngine;
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
    public abstract class SubmitCancelEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>, ISubmitHandler, ICancelHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendSubmitEvent() {
            World<TWorld>.SendEvent(new SubmitEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendCancelEvent() {
            World<TWorld>.SendEvent(new CancelEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
            });
        }

        public void OnSubmit(BaseEventData eventData) {
            if (!CanSend()) return;
            if (SendEvents) OnSendSubmitEvent();
        }

        public void OnCancel(BaseEventData eventData) {
            if (!CanSend()) return;
            if (SendEvents) OnSendCancelEvent();
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class SubmitCancelEntityGIDProvider<TWorld> : SubmitCancelEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField]
        private EntityGID entityGid;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityGid;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class SubmitCancelEntityRefProvider<TWorld, TProvider> : SubmitCancelEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField]
        private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)] get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;

        #if UNITY_EDITOR
        protected void Reset() {
            if (entityProvider == null) entityProvider = GetComponent<TProvider>();
        }
        #endif
    }
}