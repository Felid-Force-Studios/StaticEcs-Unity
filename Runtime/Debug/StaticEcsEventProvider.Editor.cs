#if UNITY_EDITOR
using System;

namespace FFS.Libraries.StaticEcs.Unity {
    
    public abstract partial class StaticEcsEventProvider<TWorld> {

        public virtual IEvent GetActualEvent(out bool cached) {
            if (RuntimeEvent.IsEmpty()) {
                cached = false;
                return EventTemplate;
            }

            var handle = World<TWorld>.Data.Handle;
            if (handle.TryGetEventsHandle(RuntimeEvent.Type, out var eventsHandle)) {
                if (RuntimeEvent.Status != EventStatus.Sent) {
                    cached = true;
                    return EventCache;
                }

                EventCache = eventsHandle.GetRaw(RuntimeEvent.InternalIdx);
                cached = false;
                return EventCache;
            }

            throw new InvalidOperationException("Event pool not registered");
        }

        public virtual bool IsCached() {
            return RuntimeEvent.Status != EventStatus.Sent;
        }

        public virtual bool ShouldShowEvent(Type type, bool runtime) {
            return !runtime || (World<TWorld>.Status == WorldStatus.Initialized
                                && World<TWorld>.Data.Handle.TryGetEventsHandle(type, out _));
        }

        public virtual void OnSelectEvent(IEvent e) {
            EventTemplate = e;
        }

        public virtual bool EventIsActual(bool runtime) {
            return (runtime && !RuntimeEvent.IsEmpty()) || EventTemplate != null;
        }

        public virtual void DeleteEvent() {
            var handle = World<TWorld>.Data.Handle;
            if (handle.TryGetEventsHandle(RuntimeEvent.Type, out var eventsHandle)) {
                eventsHandle.Delete(RuntimeEvent.InternalIdx);
            } else {
                throw new InvalidOperationException("Event pool not registered");
            }
        }

        public virtual void OnChangeEvent(IEvent newValue) {
            if (RuntimeEvent.IsEmpty()) {
                EventTemplate = newValue;
            } else {
                var handle = World<TWorld>.Data.Handle;
                if (handle.TryGetEventsHandle(RuntimeEvent.Type, out var eventsHandle)) {
                    eventsHandle.PutRaw(RuntimeEvent.InternalIdx, newValue);
                } else {
                    throw new InvalidOperationException("Event pool not registered");
                }

                EventCache = newValue;
            }
        }
    }
}
#endif
