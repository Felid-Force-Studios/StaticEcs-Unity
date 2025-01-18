using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    [DefaultExecutionOrder(short.MinValue)]
    public abstract class StaticEcsNamedContextReference<WorldType> : MonoBehaviour where WorldType : struct, IWorldType {
        [SerializeField] private string _key;
        [SerializeField] private RegistrationType _registrationType = RegistrationType.OnAwake;

        public string Key() {
            return _key;
        }

        void Awake() {
            if (_registrationType == RegistrationType.OnAwake) {
                Ecs<WorldType>.NamedContext.Set(_key, gameObject);
            }
        }

        void OnEnable() {
            if (_registrationType == RegistrationType.OnEnable) {
                Ecs<WorldType>.NamedContext.Set(_key, gameObject);
            }
        }

        void OnDisable() {
            if (_registrationType == RegistrationType.OnEnable) {
                Ecs<WorldType>.NamedContext.Remove(_key);
            }
        }

        void OnDestroy() {
            if (_registrationType == RegistrationType.OnAwake) {
                Ecs<WorldType>.NamedContext.Remove(_key);
            }
        }

        enum RegistrationType {
            OnAwake,
            OnEnable
        }
    }
}