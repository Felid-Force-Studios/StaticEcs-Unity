using System;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [DefaultExecutionOrder(ushort.MaxValue)]
    public abstract class AbstractStaticEcsProvider : MonoBehaviour {
        [SerializeField]
        public UsageType UsageType = UsageType.OnStart;
        [SerializeField]
        public OnCreateType OnCreateType = OnCreateType.None;

        [HideInInspector]
        public Vector2 Scroll;

        protected virtual void OnCreate() {
            switch (OnCreateType) {
                case OnCreateType.DestroyUnityComponent:
                    Destroy(this);
                    return;
                case OnCreateType.DestroyGameObject:
                    Destroy(gameObject);
                    return;
                case OnCreateType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum UsageType {
        OnStart, OnAwake, Manual,
    }

    public enum OnCreateType {
        None, DestroyUnityComponent, DestroyGameObject,
    }
}