using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class ComponentDrawerWrapper : ScriptableObject {
        [SerializeReference] public IComponent value;

        private static ComponentDrawerWrapper _instance;

        public static ComponentDrawerWrapper Instance {
            get {
                if (!_instance) {
                    _instance = CreateInstance<ComponentDrawerWrapper>();
                    _instance.hideFlags = HideFlags.DontSave;
                }
                return _instance;
            }
        }
    }

    internal class EventDrawerWrapper : ScriptableObject {
        [SerializeReference] public IEvent value;

        private static EventDrawerWrapper _instance;

        public static EventDrawerWrapper Instance {
            get {
                if (!_instance) {
                    _instance = CreateInstance<EventDrawerWrapper>();
                    _instance.hideFlags = HideFlags.DontSave;
                }
                return _instance;
            }
        }
    }

    internal class ContextDrawerWrapper : ScriptableObject {
        [SerializeReference] public object value;

        private static ContextDrawerWrapper _instance;

        public static ContextDrawerWrapper Instance {
            get {
                if (!_instance) {
                    _instance = CreateInstance<ContextDrawerWrapper>();
                    _instance.hideFlags = HideFlags.DontSave;
                }
                return _instance;
            }
        }
    }

    internal class ContextValueDrawerWrapper<T> : ScriptableObject {
        public T value;
    }
}
