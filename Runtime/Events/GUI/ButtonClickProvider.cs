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
    [RequireComponent(typeof(Button))]
    public abstract class ButtonClickProvider<TWorld> : GUIEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private Button _button;

        protected virtual void Awake() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(Button button) {
            World<TWorld>.SendEvent(new ButtonClickEvent {
                Ref = button,
            });
        }

        private void OnButtonClick() {
            if (!CanSend()) return;
            OnSendEvent(_button);
        }
    }
}