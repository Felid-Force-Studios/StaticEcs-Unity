using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal class EventDrawerWrapper : ScriptableObject {
        [SerializeReference] public IEvent value;

        private static readonly Dictionary<EntityId, EventDrawerWrapper> _pool = new();

        public static EventDrawerWrapper GetFor(EntityId key) {
            if (!_pool.TryGetValue(key, out var w) || w == null) {
                w = CreateInstance<EventDrawerWrapper>();
                w.hideFlags = HideFlags.DontSave;
                _pool[key] = w;
            }
            return w;
        }
    }
}
