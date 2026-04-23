using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class BoxedStructDrawerWrapper : ScriptableObject {
        [SerializeReference] public object value;

        private static readonly Dictionary<long, BoxedStructDrawerWrapper> _pool = new();

        public static BoxedStructDrawerWrapper GetFor(long key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<BoxedStructDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }
}
