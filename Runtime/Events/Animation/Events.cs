#if FFS_ECS_ANIMATION
using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimationEventEcsEvent : IEvent {
        public GameObject Ref;
        public string StringParameter;
        public int IntParameter;
        public float FloatParameter;
        public UnityEngine.Object ObjectReferenceParameter;
        public AnimationState AnimationState;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimationEventEntityEcsEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public string StringParameter;
        public int IntParameter;
        public float FloatParameter;
        public UnityEngine.Object ObjectReferenceParameter;
        public AnimationState AnimationState;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimatorStateEnterEvent : IEvent {
        public GameObject Ref;
        public AnimatorStateInfo StateInfo;
        public int LayerIndex;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimatorStateExitEvent : IEvent {
        public GameObject Ref;
        public AnimatorStateInfo StateInfo;
        public int LayerIndex;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimatorStateEnterEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public AnimatorStateInfo StateInfo;
        public int LayerIndex;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct AnimatorStateExitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public AnimatorStateInfo StateInfo;
        public int LayerIndex;
    }
}
#endif