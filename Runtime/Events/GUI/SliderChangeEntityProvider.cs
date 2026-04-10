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
    public abstract class SliderChangeEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private Slider _slider;

        protected virtual void Awake() {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(Slider slider, float value) {
            World<TWorld>.SendEvent(new SliderChangeEntityEvent {
                Ref = slider,
                EntityGID = EntityGID,
                Value = value,
            });
        }

        private void OnSliderValueChanged(float v) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEvent(_slider, v);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class SliderChangeEntityGIDProvider<TWorld> : SliderChangeEntityProvider<TWorld>
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
    public abstract class SliderChangeEntityRefProvider<TWorld, TProvider> : SliderChangeEntityProvider<TWorld>
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