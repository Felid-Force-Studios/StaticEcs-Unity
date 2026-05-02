#if FFS_ECS_PHYSICS
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CollisionEnter3DEvent : IEvent {
        public GameObject Ref;
        public Collider Collider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Velocity;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CollisionExit3DEvent : IEvent {
        public GameObject Ref;
        public Collider Collider;
        public Vector3 Velocity;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct TriggerEnter3DEvent : IEvent {
        public GameObject Ref;
        public Collider Collider;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct TriggerExit3DEvent : IEvent {
        public GameObject Ref;
        public Collider Collider;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ControllerColliderHit3DEvent : IEvent {
        public GameObject Ref;
        public Collider Collider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 MoveDirection;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CollisionEnter3DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider Collider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Velocity;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CollisionExit3DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider Collider;
        public Vector3 Velocity;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct TriggerEnter3DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider Collider;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct TriggerExit3DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider Collider;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ControllerColliderHit3DEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Collider Collider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 MoveDirection;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct Collision3DState : IComponent {
        public Collider Collider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Velocity;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct Trigger3DState : IComponent {
        public Collider Collider;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ContactEnter3DEvent : IEvent {
        public Collider ColliderA;
        public Collider ColliderB;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Impulse;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ContactExit3DEvent : IEvent {
        public Collider ColliderA;
        public Collider ColliderB;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ContactEnter3DEntityEvent : IEvent {
        public EntityGID EntityA;
        public EntityGID EntityB;
        public Collider ColliderA;
        public Collider ColliderB;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Impulse;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ContactExit3DEntityEvent : IEvent {
        public EntityGID EntityA;
        public EntityGID EntityB;
        public Collider ColliderA;
        public Collider ColliderB;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ContactCollision3DState : IComponent {
        public Collider OtherCollider;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Impulse;
    }

    public struct ContactColliderEntityMap : IResource {
        public Dictionary<EntityId, EntityGID> Map;
    }
}
#endif