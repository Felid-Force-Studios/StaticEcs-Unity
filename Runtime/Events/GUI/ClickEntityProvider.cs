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
    public abstract class ClickEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>, IPointerClickHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new ClickEntityEvent {
                Ref = gameObject,
                EntityGID = EntityGID,
                Position = data.position,
                Button = data.button,
            });
        }

        public void OnPointerClick(PointerEventData data) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEvent(data);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ClickEntityGIDProvider<TWorld> : ClickEntityProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField] private EntityGID entityGid;
        protected override EntityGID EntityGID { [MethodImpl(AggressiveInlining)] get => entityGid; }
        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ClickEntityRefProvider<TWorld, TProvider> : ClickEntityProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField] private TProvider entityProvider;
        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityProvider != null ? entityProvider.EntityGid : default;
        }
        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;
    }
}
