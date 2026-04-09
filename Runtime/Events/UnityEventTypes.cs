namespace FFS.Libraries.StaticEcs.Unity {

    public static class UnityEventTypes {

        public static void Register<TWorld>() where TWorld : struct, IWorldType {
            World<TWorld>.Types()
                .Event<ClickEvent>()
                .Event<ClickEntityEvent>()
                .Event<PointerEnterEvent>()
                .Event<PointerExitEvent>()
                .Event<PointerEnterEntityEvent>()
                .Event<PointerExitEntityEvent>()
                .Event<PointerUpEvent>()
                .Event<PointerDownEvent>()
                .Event<PointerUpEntityEvent>()
                .Event<PointerDownEntityEvent>()
                .Event<DragStartEvent>()
                .Event<DragMoveEvent>()
                .Event<DragEndEvent>()
                .Event<DragStartEntityEvent>()
                .Event<DragMoveEntityEvent>()
                .Event<DragEndEntityEvent>()
                .Event<DropEvent>()
                .Event<DropEntityEvent>()
                .Event<ScrollViewChangeEvent>()
                .Event<ScrollViewChangeEntityEvent>()
                .Event<SliderChangeEvent>()
                .Event<SliderChangeEntityEvent>()
                .Component<PointerHoverState>()
                .Component<PointerPressedState>()
                .Component<DragState>()
                .Event<MouseDownEvent>()
                .Event<MouseDownEntityEvent>()
                .Event<MouseUpEvent>()
                .Event<MouseUpEntityEvent>()
                .Event<MouseUpAsButtonEvent>()
                .Event<MouseUpAsButtonEntityEvent>()
                .Event<MouseEnterEvent>()
                .Event<MouseEnterEntityEvent>()
                .Event<MouseExitEvent>()
                .Event<MouseExitEntityEvent>()
                .Component<MouseHoverState>()
                .Component<MousePressedState>();

            #if FFS_ECS_TMP
            World<TWorld>.Types()
                .Event<DropdownChangeEvent>()
                .Event<DropdownChangeEntityEvent>()
                .Event<InputChangeEvent>()
                .Event<InputChangeEntityEvent>()
                .Event<InputEndEvent>()
                .Event<InputEndEntityEvent>();
            #endif

            #if FFS_ECS_PHYSICS
            World<TWorld>.Types()
                .Event<CollisionEnter3DEvent>()
                .Event<CollisionExit3DEvent>()
                .Event<CollisionEnter3DEntityEvent>()
                .Event<CollisionExit3DEntityEvent>()
                .Event<TriggerEnter3DEvent>()
                .Event<TriggerExit3DEvent>()
                .Event<TriggerEnter3DEntityEvent>()
                .Event<TriggerExit3DEntityEvent>()
                .Event<ControllerColliderHit3DEvent>()
                .Event<ControllerColliderHit3DEntityEvent>()
                .Component<Collision3DState>()
                .Component<Trigger3DState>()
                .Event<ContactEnter3DEvent>()
                .Event<ContactExit3DEvent>()
                .Event<ContactEnter3DEntityEvent>()
                .Event<ContactExit3DEntityEvent>()
                .Component<ContactCollision3DState>();
            #endif

            #if FFS_ECS_PHYSICS2D
            World<TWorld>.Types()
                .Event<CollisionEnter2DEvent>()
                .Event<CollisionExit2DEvent>()
                .Event<CollisionEnter2DEntityEvent>()
                .Event<CollisionExit2DEntityEvent>()
                .Event<TriggerEnter2DEvent>()
                .Event<TriggerExit2DEvent>()
                .Event<TriggerEnter2DEntityEvent>()
                .Event<TriggerExit2DEntityEvent>()
                .Component<Collision2DState>()
                .Component<Trigger2DState>();
            #endif
        }
    }
}
