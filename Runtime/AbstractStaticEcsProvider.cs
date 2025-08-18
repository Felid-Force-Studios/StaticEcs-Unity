using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public abstract class AbstractStaticEcsProvider : MonoBehaviour {
        [SerializeField] public UsageType UsageType = UsageType.OnStart;
        [SerializeField] public OnCreateType OnCreateType = OnCreateType.None;
        
        [SerializeField, HideInInspector] public string WorldEditorName;
        
        public string WorldTypeName {
            get => _worldTypeName;
            set {
                _worldTypeName = value;
                _world = null;
            }
        }
        [SerializeField, HideInInspector] internal string _worldTypeName;
        
        public IWorld World {
            get {
                if (_world == null) {
                    foreach (var typeToWorld in Worlds._worlds) {
                        if (WorldTypeName == typeToWorld.Key.FullName) {
                            _world = typeToWorld.Value;
                            break;
                        }
                    }
                }

                return _world;
            }
            set => _world = value;
        }
        internal IWorld _world;
        
        [HideInInspector] public Vector2 Scroll;
        
        public virtual void OnCreate() {
            switch (OnCreateType) {
                case OnCreateType.DestroyUnityComponent:
                    Destroy(this);
                    return;
                case OnCreateType.DestroyGameObject:
                    Destroy(gameObject);
                    return;
            }
        }
    }
    
    public enum UsageType {
        OnStart,
        OnAwake,
        Manual,
    }

    public enum OnCreateType {
        None,
        DestroyUnityComponent,
        DestroyGameObject,
    }
}