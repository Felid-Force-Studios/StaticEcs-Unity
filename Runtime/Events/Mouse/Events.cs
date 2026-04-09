using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    [Serializable]
    public struct MouseHoverState : IComponent { }

    [Serializable]
    public struct MousePressedState : IComponent { }

    [Serializable]
    public struct MouseDownEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct MouseDownEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct MouseEnterEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct MouseEnterEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct MouseExitEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct MouseExitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct MouseUpEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct MouseUpEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct MouseUpAsButtonEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct MouseUpAsButtonEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }
}