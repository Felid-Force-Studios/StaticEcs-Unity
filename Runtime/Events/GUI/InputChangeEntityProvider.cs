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
    public abstract class InputChangeEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private TMP_InputField _input;

        protected virtual void Awake() {
            _input = GetComponent<TMP_InputField>();
            _input.onValueChanged.AddListener(OnInputValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(TMP_InputField input, string value) {
            World<TWorld>.SendEvent(new InputChangeEntityEvent {
                Ref = input,
                EntityGID = EntityGID,
                Value = value,
            });
        }

        private void OnInputValueChanged(string v) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEvent(_input, v);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class InputChangeEntityGIDProvider<TWorld> : InputChangeEntityProvider<TWorld>
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
    public abstract class InputChangeEntityRefProvider<TWorld, TProvider> : InputChangeEntityProvider<TWorld>
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
#endif
