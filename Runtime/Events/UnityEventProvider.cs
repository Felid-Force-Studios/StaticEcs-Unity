using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    public enum EntityEventMode : byte {
        All,
        EventOnly,
        ComponentOnly,
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class UnityEventProvider<TWorld> : MonoBehaviour
        where TWorld : struct, IWorldType {

        [MethodImpl(AggressiveInlining)]
        protected virtual bool CanSend() => World<TWorld>.Status == WorldStatus.Initialized;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class UnityEntityEventProvider<TWorld> : UnityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField] private EntityEventMode mode = EntityEventMode.EventOnly;

        protected bool SendEvents {
            [MethodImpl(AggressiveInlining)]
            get => mode != EntityEventMode.ComponentOnly;
        }

        protected bool ManageComponents {
            [MethodImpl(AggressiveInlining)]
            get => mode != EntityEventMode.EventOnly;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityEventMode(EntityEventMode value) => mode = value;

        protected abstract EntityGID EntityGID { get; }

        [MethodImpl(AggressiveInlining)]
        protected void SetComponentOnEntity<T>(T component) where T : struct, IComponent {
            if (!EntityGID.TryUnpack<TWorld>(out var entity)) return;
            World<TWorld>.Components<T>.Instance.Set(entity, component);
        }

        [MethodImpl(AggressiveInlining)]
        protected void DeleteComponentFromEntity<T>() where T : struct, IComponent {
            if (!EntityGID.TryUnpack<TWorld>(out var entity)) return;
            World<TWorld>.Components<T>.Instance.Delete(entity);
        }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class UnityEntityGIDEventProvider<TWorld> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField]
        private EntityGID entityGid;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityGid;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityGID(EntityGID gid) => entityGid = gid;
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class UnityEntityRefEventProvider<TWorld, TProvider> : UnityEntityEventProvider<TWorld>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        [SerializeField]
        private TProvider entityProvider;

        protected override EntityGID EntityGID {
            [MethodImpl(AggressiveInlining)]
            get => entityProvider != null ? entityProvider.EntityGid : default;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetEntityProvider(TProvider provider) => entityProvider = provider;
    }
}
