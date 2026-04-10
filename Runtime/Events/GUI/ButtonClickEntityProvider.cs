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
    public abstract class ButtonClickEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private Button _button;

        protected virtual void Awake() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(Button button) {
            World<TWorld>.SendEvent(new ButtonClickEntityEvent {
                Ref = button,
                EntityGID = EntityGID,
            });
        }

        private void OnButtonClick() {
            if (!CanSend()) return;
            if (SendEvents) OnSendEvent(_button);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ButtonClickEntityGIDProvider<TWorld> : ButtonClickEntityProvider<TWorld>
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
    public abstract class ButtonClickEntityRefProvider<TWorld, TProvider> : ButtonClickEntityProvider<TWorld>
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