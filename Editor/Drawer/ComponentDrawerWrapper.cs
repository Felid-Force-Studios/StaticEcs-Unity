using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class ComponentDrawerWrapper : ScriptableObject {
        [SerializeReference] public IComponent value;

        private static readonly Dictionary<long, ComponentDrawerWrapper> _pool = new();

        public static ComponentDrawerWrapper GetFor(long key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<ComponentDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }
}
