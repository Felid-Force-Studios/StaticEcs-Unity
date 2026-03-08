#if FFS_ECS_PHYSICS2D
using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    [Serializable]
    public struct CollisionEnter2DEvent : IEvent {
        public GameObject Ref;
        public Collider2D Collider;
        public Vector2 Point;
        public Vector2 Normal;
        public Vector2 Velocity;
    }

    [Serializable]
    public struct CollisionExit2DEvent : IEvent {
        public GameObject Ref;
        public Collider2D Collider;
        public Vector2 Velocity;
    }

    [Serializable]
    public struct TriggerEnter2DEvent : IEvent {
        public GameObject Ref;
        public Collider2D Collider;
    }

    [Serializable]
    public struct TriggerExit2DEvent : IEvent {
        public GameObject Ref;
        public Collider2D Collider;
    }

    [Serializable]
    public struct CollisionEnter2DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider2D Collider;
        public Vector2 Point;
        public Vector2 Normal;
        public Vector2 Velocity;
    }

    [Serializable]
    public struct CollisionExit2DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider2D Collider;
        public Vector2 Velocity;
    }

    [Serializable]
    public struct TriggerEnter2DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider2D Collider;
    }

    [Serializable]
    public struct TriggerExit2DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider2D Collider;
    }

    [Serializable]
    public struct Collision2DState : IComponent {
        public Collider2D Collider;
        public Vector2 Point;
        public Vector2 Normal;
        public Vector2 Velocity;
    }

    [Serializable]
    public struct Trigger2DState : IComponent {
        public Collider2D Collider;
    }
}
#endif
