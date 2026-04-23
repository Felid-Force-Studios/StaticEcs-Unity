using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class ContextDrawerWrapper : ScriptableObject {
        [SerializeReference] public IResource value;

        private static readonly Dictionary<int, ContextDrawerWrapper> _pool = new();

        public static ContextDrawerWrapper GetFor(int key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<ContextDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }
}
