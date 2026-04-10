#if FFS_ECS_TMP
using System.Runtime.CompilerServices;
using TMPro;
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
    [RequireComponent(typeof(TMP_InputField))]
    public abstract class InputChangeProvider<TWorld> : GUIEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private TMP_InputField _input;

        protected virtual void Awake() {
            _input = GetComponent<TMP_InputField>();
            _input.onValueChanged.AddListener(OnInputValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(TMP_InputField input, string value) {
            World<TWorld>.SendEvent(new InputChangeEvent {
                Ref = input,
                Value = value,
            });
        }

        private void OnInputValueChanged(string v) {
            if (!CanSend()) return;
            OnSendEvent(_input, v);
        }
    }
}
#endif