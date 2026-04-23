#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class SystemDrawerWrapper : ScriptableObject {
        [SerializeReference] public ISystem value;

        private static readonly Dictionary<int, SystemDrawerWrapper> _pool = new();

        public static SystemDrawerWrapper GetFor(int key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<SystemDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }
}
#endif
