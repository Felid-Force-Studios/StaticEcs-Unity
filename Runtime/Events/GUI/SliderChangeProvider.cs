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
    [RequireComponent(typeof(Slider))]
    public abstract class SliderChangeProvider<TWorld> : GUIEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private Slider _slider;

        protected virtual void Awake() {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(Slider slider, float value) {
            World<TWorld>.SendEvent(new SliderChangeEvent {
                Ref = slider,
                Value = value,
            });
        }

        private void OnSliderValueChanged(float v) {
            if (!CanSend()) return;
            OnSendEvent(_slider, v);
        }
    }
}