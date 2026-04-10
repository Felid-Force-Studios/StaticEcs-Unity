using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ScrollViewProvider<TWorld> : GUIEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private ScrollRect _scrollView;

        protected virtual void Awake() {
            _scrollView = GetComponent<ScrollRect>();
            _scrollView.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(ScrollRect scrollRect, Vector2 value) {
            World<TWorld>.SendEvent(new ScrollViewChangeEvent {
                Ref = scrollRect,
                Value = value,
            });
        }

        private void OnScrollViewValueChanged(Vector2 v) {
            if (!CanSend()) return;
            OnSendEvent(_scrollView, v);
        }
    }
}