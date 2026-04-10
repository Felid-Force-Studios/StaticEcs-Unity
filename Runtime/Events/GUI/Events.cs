#if FFS_ECS_TMP
using TMPro;
#endif
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FFS.Libraries.StaticEcs.Unity {

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ClickEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragStartEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragMoveEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragEndEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerEnterEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerExitEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerUpEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerDownEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DropEvent : IEvent {
        public GameObject Ref;
        public PointerEventData.InputButton Button;
    }

    #if FFS_ECS_TMP
    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DropdownChangeEvent : IEvent {
        public TMP_Dropdown Ref;
        public int Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct InputChangeEvent : IEvent {
        public TMP_InputField Ref;
        public string Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct InputEndEvent : IEvent {
        public TMP_InputField Ref;
        public string Value;
    }
    #endif

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ScrollViewChangeEvent : IEvent {
        public ScrollRect Ref;
        public Vector2 Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct SliderChangeEvent : IEvent {
        public Slider Ref;
        public float Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ClickEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragStartEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragMoveEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragEndEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerEnterEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerExitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerUpEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct PointerDownEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DropEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public PointerEventData.InputButton Button;
    }

    #if FFS_ECS_TMP
    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DropdownChangeEntityEvent : IEvent {
        public TMP_Dropdown Ref;
        public EntityGID EntityGID;
        public int Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct InputChangeEntityEvent : IEvent {
        public TMP_InputField Ref;
        public EntityGID EntityGID;
        public string Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct InputEndEntityEvent : IEvent {
        public TMP_InputField Ref;
        public EntityGID EntityGID;
        public string Value;
    }
    #endif

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ScrollViewChangeEntityEvent : IEvent {
        public ScrollRect Ref;
        public EntityGID EntityGID;
        public Vector2 Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct SliderChangeEntityEvent : IEvent {
        public Slider Ref;
        public EntityGID EntityGID;
        public float Value;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct SubmitEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct SubmitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CancelEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct CancelEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ButtonClickEvent : IEvent {
        public Button Ref;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct ButtonClickEntityEvent : IEvent {
        public Button Ref;
        public EntityGID EntityGID;
    }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public readonly struct PointerHoverState : ITag { }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public readonly struct PointerPressedState : ITag { }

    [Serializable, StaticEcsEditorColor(StaticEcsEditorColorAttribute.SystemColor)]
    public struct DragState : IComponent {
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }
}