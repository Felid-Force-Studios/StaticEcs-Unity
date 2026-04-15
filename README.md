<p align="center">
  <img src="https://raw.githubusercontent.com/Felid-Force-Studios/StaticEcs/master/docs/fulllogo.png" alt="Static ECS" width="100%">
  <br><br>
  <a href="./README.md"><img src="https://img.shields.io/badge/EN-English-blue?style=flat-square" alt="English"></a>
  <a href="./README_RU.md"><img src="https://img.shields.io/badge/RU-Русский-blue?style=flat-square" alt="Русский"></a>
  <a href="./README_ZH.md"><img src="https://img.shields.io/badge/ZH-中文-blue?style=flat-square" alt="中文"></a>
  <br><br>
  <img src="https://img.shields.io/badge/version-2.1.1-blue?style=for-the-badge" alt="Version">
  <a href="https://felid-force-studios.github.io/StaticEcs/en/"><img src="https://img.shields.io/badge/Docs-documentation-blueviolet?style=for-the-badge" alt="Documentation"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs"><img src="https://img.shields.io/badge/Core-framework-green?style=for-the-badge" alt="Core framework"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs-Showcase"><img src="https://img.shields.io/badge/Showcase-examples-yellow?style=for-the-badge" alt="Showcase"></a>
</p>

# Static ECS - C# Entity component system framework - Unity module

## Table of Contents
* [Contacts](#contacts)
* [Installation](#installation)
* [Guide](#guide)
  * [Connection](#connection)
  * [Entity providers](#entity-providers)
  * [Event providers](#event-providers)
  * [Unity event providers](#unity-event-providers)
  * [Templates](#templates)
  * [Static ECS view window](#static-ecs-view-window)
  * [Settings](#settings)
* [Questions](#questions)
* [License](#license)


# Contacts
* [Telegram](https://t.me/felid_force_studios)

# Installation
Must also be installed [StaticEcs](https://github.com/Felid-Force-Studios/StaticEcs)
* ### As source code
  From the release page or as an archive from the branch. In the `master` branch there is a stable tested version
* ### Installation for Unity
  - As a git module in Unity PackageManager     
    `https://github.com/Felid-Force-Studios/StaticEcs-Unity.git`
  - Or adding to the manifest `Packages/manifest.json`  
    `"com.felid-force-studios.static-ecs-unity": "https://github.com/Felid-Force-Studios/StaticEcs-Unity.git"`


# Guide
The module provides additional integration options with the Unity engine:

### Connection:
ECS runtime data monitoring and management window  
To connect worlds and systems to the editor window it is necessary to call a special method when initializing the world and systems  
specifying the world or systems required
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientSystems.Create();
        
        EcsDebug<ClientWorldType>.AddWorld<ClientSystemsType>();
        
        ClientWorld.Initialize();
        ClientSystems.Initialize();
```

To add additional system groups to the debug window, use `AddSystem` after the systems are initialized:
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientSystems.Create();
        ClientAdditionalSystems.Create();
        
        EcsDebug<ClientWorldType>.AddWorld<ClientSystemsType>();
        
        ClientWorld.Initialize();
        ClientSystems.Initialize();
        ClientAdditionalSystems.Initialize();
        
        EcsDebug<ClientWorldType>.AddSystem<ClientAdditionalSystemsType>();
```
Note: `AddWorld` must be called before `Initialize` (it registers the debug system), while `AddSystem` must be called after `Initialize` (systems must already be initialized)

### Entity providers:
A script that adds the ability to configure an entity in the Unity editor and automatically create it in the ECS world  
Add the `StaticEcsEntityProvider` script to an object in the scene:

![EntityProvider.png](Readme%2FEntityProvider.png)

`Usage type` - creation type, automatically when `Start()`, `Awake()` is called, or manually  
`On create type` - action after creating the provider, delete the `StaticEcsEntityProvider` component from the object, delete the entire object, or nothing  
`On destroy type` - action when destroying the provider, destroy the entity or nothing  
`Prefab` - allows referring to the provider prefab, while changing component data will be blocked  
`Entity GID` - global entity identifier in runtime  
`Disable entity on create` - disables entity immediately after creation  
`On enable and disable` - enables/disables entity when GameObject is enabled/disabled  
`Entity Type` - entity type  
`Cluster ID` - cluster identifier in runtime  
`Components` - component data  
`Tags` - entity tags

### Event providers:
A script that adds the ability to configure an event in the Unity editor and automatically send it to the ECS world  
Add the `StaticEcsEventProvider` script to an object in the scene:

![EventProvider.png](Readme%2FEventProvider.png)

`Usage type` - creation type, automatically when `Start()`, `Awake()` is called, or manually  
`On create type` - action after creation, delete the `StaticEcsEventProvider` component from the object, delete the entire object, or nothing  
`World` - type of world in which the event will be sent  
`Type` - event type


### Unity event providers:
A system of providers for automatic forwarding of Unity events (physics, GUI) to the ECS world  
Providers intercept Unity callbacks (OnCollisionEnter, IPointerClickHandler, etc.) and send corresponding ECS events  

#### Type registration

Before using providers, you must register event and component types in the world  
Call `UnityEventTypes.Register<TWorld>()` between `Create()` and `Initialize()`:
```csharp
        ClientWorld.Create(WorldConfig.Default());
        UnityEventTypes.Register<ClientWorldType>(); // Registers all events and components
        ClientWorld.Initialize();
```
The method automatically registers all module event and component types, respecting installed modules (Physics, Physics2D, TextMeshPro)  

Alternatively, you can use `RegisterAll` to automatically register all types from specified assemblies via reflection:
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientWorld.Types().RegisterAll(typeof(MyComponent).Assembly); // All IComponent, IEvent, ITag from assembly
        ClientWorld.Initialize();
```
`RegisterAll` without parameters uses the calling assembly. Multiple assemblies can be passed  

#### Three operating modes

| Mode | Class suffix | Description |
|---|---|---|
| Without entity | `Provider<TWorld>` | Sends event to the ECS world without entity binding |
| EntityGID | `EntityGIDProvider<TWorld>` | Binds to entity via EntityGID field |
| EntityRef | `EntityRefProvider<TWorld, TProvider>` | Binds via reference to StaticEcsEntityProvider |

**Mode without entity** - sends the event without EntityGID. Suitable for objects not associated with ECS entities  

**EntityGID mode** - stores EntityGID as a serializable field. Sends event with EntityGID  

**EntityRef mode** - stores a reference to a `StaticEcsEntityProvider` component. Gets EntityGID from the provider at runtime. Convenient when the entity provider is on the same or adjacent object  

#### EntityEventMode

Entity providers (EntityGID/EntityRef) have a configurable `EntityEventMode` field in the Inspector that controls behavior:

| Mode | Description |
|---|---|
| `All` | Sends events **and** manages state components |
| `EventOnly` | Only sends ECS events, without component management (default) |
| `ComponentOnly` | Only manages state components on the entity, without sending events |

Setting from code:
```csharp
GetComponent<MyCollision3DEntityGID>().SetEntityEventMode(EntityEventMode.All);
```

#### Generating providers

Since Unity does not support serialization of open generic types, you need to create sealed subclasses for a specific world  

For quick generation use: `Assets/Create/Static ECS/Providers`

The generation window allows you to configure:
- **World** - the world type for which providers are generated
- **Namespace** - namespace for generated classes
- **Prefix** - class name prefix (defaults to world name)
- **Event providers** - checkboxes for selecting event categories:
  - GUI - basic GUI events (Click, Drag, Drop, Pointer, ScrollView, Slider, SubmitCancel, ButtonClick)
  - TextMeshPro - TMP widget events (Dropdown, Input)
  - Mouse - mouse events (MouseDownUp, MouseEnterExit, MouseUpAsButton)
  - Physics 3D - 3D physics events (Collision, Trigger, ControllerColliderHit)
  - Physics 2D - 2D physics events (Collision, Trigger)
  - Animation - animation events (AnimationEvent, StateMachineBehaviour, StateMachineBehaviourLinker)

As a result, sealed classes ready to use in Unity Inspector will be generated  
For each event type, 3 classes are generated (one for each mode):
```
{Prefix}Click          : ClickProvider<TWorld>              // without entity
{Prefix}ClickEntityGID : ClickEntityGIDProvider<TWorld>     // with EntityGID
{Prefix}ClickEntityRef : ClickEntityRefProvider<TWorld, TProvider> // with provider
```

#### Supported events

**Physics 3D** (requires `com.unity.modules.physics`):

| Provider | Unity Callback | Events | State component |
|---|---|---|---|
| Collision3D | OnCollisionEnter/Exit | `CollisionEnter3DEvent`, `CollisionExit3DEvent` | `Collision3DState` |
| Trigger3D | OnTriggerEnter/Exit | `TriggerEnter3DEvent`, `TriggerExit3DEvent` | `Trigger3DState` |
| ControllerColliderHit3D | OnControllerColliderHit | `ControllerColliderHit3DEvent` | - |

**Physics 3D — ContactEvent** (requires `com.unity.modules.physics`, Unity 2022.2+):

Centralized high-performance alternative to per-object MonoBehaviour callbacks. Uses `Physics.ContactEvent` — a batch callback receiving all contacts in the scene in a single call. Does not require a MonoBehaviour on every physics object.

**Architecture:**
- **Listener** — one per scene. Subscribes to `Physics.ContactEvent` and processes all contacts centrally
- **ContactColliderProvider** — on each GameObject with colliders (entity variant only). Registers colliders in the mapping

**Two listener variants:**

| Listener | Events | State component | Entity mapping |
|---|---|---|---|
| `ContactEventListener<TWorld>` | `ContactEnter3DEvent`, `ContactExit3DEvent` | - | Not required |
| `ContactEventEntityListener<TWorld>` | `ContactEnter3DEntityEvent`, `ContactExit3DEntityEvent` | `ContactCollision3DState` | Via `ContactColliderEntityMap` |

`ContactEventListener<TWorld>` — sends events with collider data only, without entity binding. One component on a scene manager object.

`ContactEventEntityListener<TWorld>` — sends events with EntityGID of both bodies, using `ContactColliderEntityMap` world resource for Collider InstanceID → EntityGID mapping. One component on a scene manager object. Options: `sendNonEntityEvents`, `manageComponents`.

**Collider registration (entity variant only):**

`ContactColliderProvider<TWorld>` — MonoBehaviour placed on each GameObject with colliders. Registers colliders in `ContactColliderEntityMap` and automatically sets `providesContacts = true`. Two variants:
- `ContactColliderGIDProvider<TWorld>` — serialized EntityGID
- `ContactColliderRefProvider<TWorld, TProvider>` — reference to StaticEcsEntityProvider

The `ContactColliderEntityMap` resource is created lazily on first provider registration.

Contact events contain data about both colliders:
```csharp
public struct ContactEnter3DEntityEvent : IEvent {
    public EntityGID EntityA;
    public EntityGID EntityB;
    public Collider ColliderA;
    public Collider ColliderB;
    public Vector3 Point;
    public Vector3 Normal;
    public Vector3 Impulse;
}
```

> **Note:** `Physics.ContactEvent` only reports actual collisions, not triggers. For triggers, use the standard `Trigger3D` providers.

**Physics 2D** (requires `com.unity.modules.physics2d`):

| Provider | Unity Callback | Events | State component |
|---|---|---|---|
| Collision2D | OnCollisionEnter2D/Exit2D | `CollisionEnter2DEvent`, `CollisionExit2DEvent` | `Collision2DState` |
| Trigger2D | OnTriggerEnter2D/Exit2D | `TriggerEnter2DEvent`, `TriggerExit2DEvent` | `Trigger2DState` |

**GUI**:

| Provider | Unity Interface | Events | State component |
|---|---|---|---|
| Click | IPointerClickHandler | `ClickEvent` | - |
| PointerEnterExit | IPointerEnter/ExitHandler | `PointerEnterEvent`, `PointerExitEvent` | `PointerHoverState` |
| PointerUpDown | IPointerUp/DownHandler | `PointerUpEvent`, `PointerDownEvent` | `PointerPressedState` |
| Drag | IBeginDrag/IDrag/IEndDragHandler | `DragStartEvent`, `DragMoveEvent`, `DragEndEvent` | `DragState` |
| Drop | IDropHandler | `DropEvent` | - |
| ScrollView | ScrollRect.onValueChanged | `ScrollViewChangeEvent` | - |
| SliderChange | Slider.onValueChanged | `SliderChangeEvent` | - |
| SubmitCancel | ISubmitHandler/ICancelHandler | `SubmitEvent`, `CancelEvent` | - |
| ButtonClick | Button.onClick | `ButtonClickEvent` | - |

**GUI TMP** (requires `com.unity.textmeshpro` or `com.unity.ugui` >= 2.0.0):

| Provider | Unity Callback | Events |
|---|---|---|
| DropdownChange | TMP_Dropdown.onValueChanged | `DropdownChangeEvent` |
| InputChange | TMP_InputField.onValueChanged | `InputChangeEvent` |
| InputEnd | TMP_InputField.onEndEdit | `InputEndEvent` |

**Mouse** (requires a collider on the object):

| Provider | Unity Callback | Events | State component |
|---|---|---|---|
| MouseDownUp | OnMouseDown/Up | `MouseDownEvent`, `MouseUpEvent` | `MousePressedState` |
| MouseEnterExit | OnMouseEnter/Exit | `MouseEnterEvent`, `MouseExitEvent` | `MouseHoverState` |
| MouseUpAsButton | OnMouseUpAsButton | `MouseUpAsButtonEvent` | - |

**Animation** (requires `com.unity.modules.animation`):

| Provider | Unity Callback | Events | State component |
|---|---|---|---|
| AnimationEvent | Animation Event (clip) | `AnimationEventEcsEvent` | - |
| StateMachineBehaviour | OnStateEnter/Exit | `AnimatorStateEnterEvent`, `AnimatorStateExitEvent` | - |

`AnimationEventProvider` — place on the same GameObject as the Animator. In the animation clip, set the event function name to `OnAnimationEvent`. The event carries `stringParameter`, `intParameter`, `floatParameter`, `objectReferenceParameter` and `animationState`.

`StaticEcsStateMachineBehaviour` — add to Animator Controller states. Not a MonoBehaviour — inherits from `StateMachineBehaviour`. Entity variant (`StaticEcsEntityStateMachineBehaviour`) stores a serialized `EntityGID` field.

`StaticEcsStateMachineBehaviourLinker` — a MonoBehaviour placed on the same GameObject as the Animator. Stores a reference to `StaticEcsEntityProvider` and on `Start()` automatically calls `SetEntityGID()` on all `StaticEcsEntityStateMachineBehaviour` found in the Animator Controller. Has a public `Link()` method for manual re-linking (e.g. after changing entities at runtime).

For entity modes, events have the `Entity` suffix: `ClickEntityEvent`, `CollisionEnter3DEntityEvent`, `MouseDownEntityEvent`, etc.  
These events contain an additional `EntityGID` field

#### State components

State components are automatically managed by entity providers (EntityGID/EntityRef):  
- Added to the entity on Enter event
- Removed from the entity on Exit event
- Updated on Move/Update event (e.g. `DragState`)

```csharp
public struct Collision3DState : IComponent {
    public Collider Collider;
    public Vector3 Point;
    public Vector3 Normal;
    public Vector3 Velocity;
}

public struct DragState : IComponent {
    public Vector2 Position;
    public int PointerId;
    public Vector2 Delta;
    public PointerEventData.InputButton Button;
}

public struct PointerHoverState : IComponent { }
public struct PointerPressedState : IComponent { }
```

#### Validation

All providers check `CanSend()` before sending:
- **Base check**: world is initialized (`World<TWorld>.Status == WorldStatus.Initialized`)
- **GUI check**: additionally checks that `Selectable.interactable == true` (if assigned)
- **Entity check**: `EntityGID.TryUnpack<TWorld>()` verifies the entity is alive before setting/deleting components

#### Customization

All event sending and component management methods are `virtual` and can be overridden:

```csharp
public sealed class MyCollision3D : Collision3DProvider<MyWorldType> {
    protected override bool CanSend() {
        return base.CanSend() && gameObject.activeInHierarchy;
    }

    protected override void OnSendEnterEvent(Collision data) {
        // Custom logic instead of or in addition to base
        base.OnSendEnterEvent(data);
    }
}
```

For entity providers, the following are also available:
```csharp
protected override void OnAddComponent(Collision data) { ... }
protected override void OnRemoveComponent() { ... }
```

#### Setting EntityGID / EntityProvider from code

```csharp
// For EntityGID providers
GetComponent<MyCollision3DEntityGID>().SetEntityGID(entityGid);

// For EntityRef providers
GetComponent<MyCollision3DEntityRef>().SetEntityProvider(entityProvider);
```

#### Usage example

```csharp
using W = World<MyWorldType>;

// 1. Generate providers: Assets/Create/Static ECS/Providers
// 2. Add the required provider component to a GameObject
// 3. Register event receivers and handle events in a system:

public struct HandleClickSystem : IInitSystem, IUpdateSystem {
    private EventReceiver<MyWorldType, ClickEvent> _receiver;

    public void Init() {
        _receiver = W.RegisterEventReceiver<ClickEvent>();
    }

    public void Update() {
        foreach (var evt in _receiver) {
            ref var data = ref evt.Value;
            Debug.Log($"Click on {data.Ref.name} at {data.Position}");
        }
    }
}

// Or for entity events:
public struct HandleCollisionSystem : IInitSystem, IUpdateSystem {
    private EventReceiver<MyWorldType, CollisionEnter3DEntityEvent> _receiver;

    public void Init() {
        _receiver = W.RegisterEventReceiver<CollisionEnter3DEntityEvent>();
    }

    public void Update() {
        foreach (var evt in _receiver) {
            if (evt.Value.EntityGID.TryUnpack<MyWorldType>(out var entity)) {
                // Handle collision for a specific entity
            }
        }
    }
}

// Or use state components (EntityEventMode.ComponentOnly / EntityEventMode.All):
// Components are automatically added on enter and removed on exit
public struct HandleCollisionStateSystem : IUpdateSystem {
    public void Update() {
        foreach (var entity in W.Query<All<Collision3DState>>().Entities()) {
            ref var state = ref entity.Ref<Collision3DState>();
            Debug.Log($"Entity is colliding with {state.Collider.name}, velocity: {state.Velocity}");
        }
    }
}
```

> **Note:** State components are fully compatible with [change tracking](https://felid-force-studios.github.io/StaticEcs/en/features/tracking.html). Components are added/removed from Unity callbacks (FixedUpdate), and tracking bits are written to the current tick. Systems in both FixedUpdate and Update will see these changes as long as `W.Tick()` is called at the end of Update. For example, you can use `AllAdded<Collision3DState>` to detect the moment a collision begins:
> ```csharp
> foreach (var entity in W.Query<All<Collision3DState>, AllAdded<Collision3DState>>().Entities()) {
> }
> ```

## Templates:
Class generators are available in the `Assets/Create/Static ECS/` asset creation menu

### Providers
Generator for sealed providers for a specific world  
See [Unity event providers](#unity-event-providers) section for details

## Static ECS view window:
![WindowMenu.png](Readme%2FWindowMenu.png)

### Entities/Table - entity view table

![EntitiesTable.png](Readme%2FEntitiesTable.png)
- `Filter` allows select the necessary entities
- `Entity GID` allows to find an entity by its global identifier
- `Select` allows to select the columns to be displayed
- `Show data` allows to select the data to be displayed in the columns
- `Max entities result` maximum number of displayed entities

To display component data in a table, you must: set the `StaticEcsEditorTableValue` attribute in the component for field or property
```csharp
public struct Position : IComponent {
    [StaticEcsEditorTableValue]
    public Vector3 Val;
}
```
To display a different component name, you must set the `StaticEcsEditorName` attribute on the component
```csharp
[StaticEcsEditorName("My velocity")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```
To set the color of the component, you must set the `StaticEcsEditorColor` attribute in the component (you can set RGB or HEX color)
```csharp
[StaticEcsEditorColor("f7796a")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```

entity control buttons are also available  
- eye icon - open the entity for viewing
- lock - lock the entity in the table
- trash - destroy the entity in the world


### Viewer - entity view window
Displays all entity data with the ability to modify, add and delete components

![EntitiesViewer.png](Readme%2FEntitiesViewer.png)

By default, only **public** object fields marked with the attribute `[Serializable]`
- To display a private field, you must mark it with the attribute `[StaticEcsEditorShow]`
- To hide a public field, you must mark it with the attribute `[StaticEcsEditorHide]`
- To disable value editing in play mode, you can mark it with the attribute `[StaticEcsEditorRuntimeReadOnly]`
```csharp
public struct SomeComponent : IComponent {
    [StaticEcsEditorShow]
    [StaticEcsEditorRuntimeReadOnly]
    private int _showData;
    
    [StaticEcsEditorHide]
    public int HideData;
}
```

### Entities/Builder - entity constructor
Allows you to customize and create a new entity at runtime (Similar to entity provider)

![EntitiesBuilder.png](Readme%2FEntitiesBuilder.png)

### Stats - statistics window
Displays all world, component and event data

![Stats.png](Readme%2FStats.png)

### Events/Table - event table
Displays recent events, their details and the number of subscribers who have not read the event

![EventsTable.png](Readme%2FEventsTable.png)

Events marked in yellow mean they are suppressed  
Events marked in gray mean that they have been read by all subscribers

To display these components in a table, you must set the `StaticEcsEditorTableValue` attribute in the event for field or property
```csharp
public struct DamageEvent : IEvent {
    public float Val;

    [StaticEcsEditorTableValue]
    public string ShowData => $"Damage {Val}";
}
```
The `StaticEcsEditorColor` attribute must be set to set the event color (can be set to RGB or HEX color)
```csharp
[StaticEcsEditorColor("f7796a")]
public struct DamageEvent : IEvent {

}
```
The `StaticEcsIgnoreEvent` attribute must be set to ignore the event in the editor
```csharp
[StaticEcsIgnoreEvent]
public struct DamageEvent : IEvent {

}
```

### Viewer - event viewer
Allows you to view and modify (unread only) event data

![EventsViewer.png](Readme%2FEventsViewer.png)

### Events/Builder - event constructor
Allows you to configure and create a new event at runtime

![EventsBuilder.png](Readme%2FEventsBuilder.png)


### Systems
Displays all systems in the order in which they are executed  
Allows turning systems on and off during runtime    
Displays the average execution time of each system  

![Systems.png](Readme%2FSystems.png)

### Settings
Allows you to configure the editor window behavior. Settings are stored in a `StaticEcsViewConfig` ScriptableObject asset that is automatically created on first use.

- **Config asset** — reference to the active config. You can create multiple configs via `Assets/Create/Static ECS/View Config` and switch between them
- **Component Foldouts** — controls automatic expansion of component foldouts on entities:
  - `ExpandAll` — all components are expanded by default
  - `CollapseAll` — all components are collapsed by default
  - `Custom` — only selected component types are auto-expanded
- **Reset config to defaults** — resets all settings for the current world to defaults

Settings are persisted between sessions. The following state is saved:
- Selected tab, draw rate
- Entity table: visible columns, sort column, pinned entities, filters, max entity count
- Event table: event type filters, auto-scroll mode
- Stats: fragmentation threshold, show unregistered toggle
- Systems: max nesting depth for system property display

Settings are auto-saved periodically (every 30 seconds) and on play mode exit.

# Questions
### How to create a custom drawing method for a type?
To implement your own editor for a specific type, create a `PropertyDrawer` with the `[CustomPropertyDrawer]` attribute in the Editor folder of your project  

Example:
```csharp
[CustomPropertyDrawer(typeof(MyStruct))]
public class MyStructPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        // Custom drawing logic
        EditorGUI.EndProperty();
    }
}
```

### How to use attributes without having a dependency on this module?
It is necessary to copy the attributes by saving the namespace from `\Runtime\Attributes.cs`, after that the attributes will be correctly detected by the editor.


# License
[MIT license](./LICENSE.md)
