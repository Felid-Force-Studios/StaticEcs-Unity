using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class ComponentDrawerWrapper : ScriptableObject {
        [SerializeReference] public IComponent value;

        private static readonly Dictionary<int, ComponentDrawerWrapper> _pool = new();

        public static ComponentDrawerWrapper GetFor(int key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<ComponentDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }

    internal class EventDrawerWrapper : ScriptableObject {
        [SerializeReference] public IEvent value;

        private static readonly Dictionary<int, EventDrawerWrapper> _pool = new();

        public static EventDrawerWrapper GetFor(int key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<EventDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
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
