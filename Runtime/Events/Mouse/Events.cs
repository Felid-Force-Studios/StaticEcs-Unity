using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public readonly struct MouseHoverState : ITag { }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public readonly struct MousePressedState : ITag { }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseDownEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseDownEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseEnterEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseEnterEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseExitEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseExitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseUpEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseUpEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseUpAsButtonEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct MouseUpAsButtonEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }
}