#if UNITY_EDITOR
using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    
    public partial class StaticEcsEventProvider {

        public IEvent GetActualEvent(out bool cached) {
            if (RuntimeEvent.IsEmpty()) {
                cached = false;
                return EventTemplate;
            }

            if (World.Events().TryGetPool(RuntimeEvent.Type, out var pool)) {
                if (RuntimeEvent.Status != EventStatus.Sent) {
                    cached = true;
                    return EventCache;
                }

                EventCache = pool.GetRaw(RuntimeEvent.InternalIdx);
                cached = false;
                return EventCache;
            }
            
            throw new StaticEcsException("Event pool not registered");
        }

        public bool IsCached() {
            return RuntimeEvent.Status != EventStatus.Sent;
        }
        
        public bool ShouldShowEvent(Type type, bool runtime) {
            return !runtime || World.Events().TryGetPool(type, out var _);
        }
        
        public void OnSelectEvent(IEvent e) {
            EventTemplate = e;
        }

        public bool EventIsActual(bool runtime) {
            return (runtime && !RuntimeEvent.IsEmpty()) || EventTemplate != null;
        }

        public void DeleteEvent() {
            if (World.Events().TryGetPool(RuntimeEvent.Type, out var pool)) {
                pool.Del(RuntimeEvent.InternalIdx);
            } else {
                throw new StaticEcsException("Event pool not registered");
            }
        }

        public void OnChangeEvent(IEvent newValue) {
            if (RuntimeEvent.IsEmpty()) {
                EventTemplate = newValue;
            } else {
                if (World.Events().TryGetPool(RuntimeEvent.Type, out var pool)) {
                    pool.PutRaw(RuntimeEvent.InternalIdx, newValue);
                } else {
                    throw new StaticEcsException("Event pool not registered");
                }

                EventCache = newValue;
            }
        }
    }
}
#endif