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
    public abstract class ScrollViewEntityProvider<TWorld> : GUIEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        private ScrollRect _scrollView;

        protected virtual void Awake() {
            _scrollView = GetComponent<ScrollRect>();
            _scrollView.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

        [MethodImpl(AggressiveInlining)]
        protected virtual void OnSendEvent(ScrollRect scrollRect, Vector2 value) {
            World<TWorld>.SendEvent(new ScrollViewChangeEntityEvent {
                Ref = scrollRect,
                EntityGID = EntityGID,
                Value = value,
            });
        }

        private void OnScrollViewValueChanged(Vector2 v) {
            if (!CanSend()) return;
            if (SendEvents) OnSendEvent(_scrollView, v);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ScrollViewEntityGIDProvider<TWorld> : ScrollViewEntityProvider<TWorld>
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
    public abstract class ScrollViewEntityRefProvider<TWorld, TProvider> : ScrollViewEntityProvider<TWorld>
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
