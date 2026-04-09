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
    public class MouseUpAsButtonProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType
    {

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnMouseUpAsButtonEvent()
        {
            World<TWorld>.SendEvent(new MouseUpAsButtonEvent
            {
                Ref = gameObject,
            });
        }

        private void OnMouseUpAsButton()
        {
            if (!CanSend()) return;
            OnMouseUpAsButtonEvent();
        }
    }
}
