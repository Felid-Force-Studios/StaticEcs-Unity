#if FFS_ECS_TMP
using TMPro;
#endif
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FFS.Libraries.StaticEcs.Unity {

    [Serializable]
    public struct ClickEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragStartEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragMoveEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragEndEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct PointerEnterEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct PointerExitEvent : IEvent {
        public GameObject Ref;
    }

    [Serializable]
    public struct PointerUpEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct PointerDownEvent : IEvent {
        public GameObject Ref;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DropEvent : IEvent {
        public GameObject Ref;
        public PointerEventData.InputButton Button;
    }

    #if FFS_ECS_TMP
    [Serializable]
    public struct DropdownChangeEvent : IEvent {
        public TMP_Dropdown Ref;
        public int Value;
    }

    [Serializable]
    public struct InputChangeEvent : IEvent {
        public TMP_InputField Ref;
        public string Value;
    }

    [Serializable]
    public struct InputEndEvent : IEvent {
        public TMP_InputField Ref;
        public string Value;
    }
    #endif

    [Serializable]
    public struct ScrollViewChangeEvent : IEvent {
        public ScrollRect Ref;
        public Vector2 Value;
    }

    [Serializable]
    public struct SliderChangeEvent : IEvent {
        public Slider Ref;
        public float Value;
    }

    [Serializable]
    public struct ClickEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragStartEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragMoveEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DragEndEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct PointerEnterEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct PointerExitEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
    }

    [Serializable]
    public struct PointerUpEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct PointerDownEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public Vector2 Position;
        public int PointerId;
        public PointerEventData.InputButton Button;
    }

    [Serializable]
    public struct DropEntityEvent : IEvent {
        public GameObject Ref;
        public EntityGID EntityGID;
        public PointerEventData.InputButton Button;
    }

    #if FFS_ECS_TMP
    [Serializable]
    public struct DropdownChangeEntityEvent : IEvent {
        public TMP_Dropdown Ref;
        public EntityGID EntityGID;
        public int Value;
    }

    [Serializable]
    public struct InputChangeEntityEvent : IEvent {
        public TMP_InputField Ref;
        public EntityGID EntityGID;
        public string Value;
    }

    [Serializable]
    public struct InputEndEntityEvent : IEvent {
        public TMP_InputField Ref;
        public EntityGID EntityGID;
        public string Value;
    }
    #endif

    [Serializable]
    public struct ScrollViewChangeEntityEvent : IEvent {
        public ScrollRect Ref;
        public EntityGID EntityGID;
        public Vector2 Value;
    }

    [Serializable]
    public struct SliderChangeEntityEvent : IEvent {
        public Slider Ref;
        public EntityGID EntityGID;
        public float Value;
    }

    [Serializable]
    public struct PointerHoverState : IComponent { }

    [Serializable]
    public struct PointerPressedState : IComponent { }

    [Serializable]
    public struct DragState : IComponent {
        public Vector2 Position;
        public int PointerId;
        public Vector2 Delta;
        public PointerEventData.InputButton Button;
    }
}
