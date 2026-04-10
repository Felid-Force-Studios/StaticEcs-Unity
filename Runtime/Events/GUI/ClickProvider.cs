using System.Runtime.CompilerServices;
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
    public abstract class ClickProvider<TWorld> : GUIEventProvider<TWorld>, IPointerClickHandler
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(PointerEventData data) {
            World<TWorld>.SendEvent(new ClickEvent {
                Ref = gameObject,
                Position = data.position,
                Button = data.button,
            });
        }

        public void OnPointerClick(PointerEventData data) {
            if (!CanSend()) return;
            OnSendEvent(data);
        }
    }
}