![Version](https://img.shields.io/badge/version-1.1.0-blue.svg?style=for-the-badge)

### LANGUAGE
[RU](./README_RU.md)
[EN](./README.md)
___

### [Static ECS](https://github.com/Felid-Force-Studios/StaticEcs)

# Static ECS - C# Entity component system framework - Unity module
#### Limitations and Features:
> - Not thread safe
> - There may be minor API changes

## Table of Contents
* [Contacts](#contacts)
* [Installation](#installation)
* [Guide](#guide)
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


# Guid
The module provides additional integration options with the Unity engine:

### Connection:
ECS runtime data monitoring and management window  
To connect worlds and systems to the editor window it is necessary to call a special method when initializing the world and systems  
specifying the world or systems required
```csharp
        ClientWorld.Create(WorldConfig.Default());
        //...
        EcsDebug<ClientWorldType>.AddWorld(); // Between creation and initialization
        //...
        ClientWorld.Initialize();
        
        ClientSystems.Create();
        //...
        ClientSystems.Initialize();
        EcsDebug<ClientWorldType>.AddSystem<ClientSystemsType>(); // After initialization
```

### Entity providers:
A script that adds the ability to configure an entity in the Unity editor and automatically create it in the ECS world  
Add the `StaticEcsEntityProvider` script to an object in the scene:

![EntityProvider.png](Readme%2FEntityProvider.png)

`Usage type` - creation type, automatically when `Start()`, `Awake()` is called, or manually  
`On create type` - action after creating the provider, delete the `StaticEcsEntityProvider` component from the object, delete the entire object, or nothing  
`On destroy type` - action when destroying the provider, destroy the entity or nothing  
`Prefab` - allows referring to the provider prefab, while changing component data will be blocked  
`Wotld` - the type of world in which the entity will be created  
`Entity ID` - entity identifier in runtime  
`Entity GID` - global entity identifier in runtime  
`Standard components` - standard component data
`Components` - component data
`Tags` - entity tags  
`Masks` - entity masks

### Event providers:
A script that adds the ability to configure an event in the Unity editor and automatically send it to the ECS world  
Add the `StaticEcsEventProvider` script to an object in the scene:

![EventProvider.png](Readme%2FEventProvider.png)

`Usage type` - creation type, automatically when `Start()`, `Awake()` is called, or manually  
`On create type` - action after creation, delete the `StaticEcsEventProvider` component from the object, delete the entire object, or nothing  
`Wotld` - type of world in which the event will be sent  
`Type` - event type

### Templates:
Class generators are optionally available in the `Assets/Create/Static ECS/` asset creation menu

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

### Settings - editor settings

![Settings.png](Readme%2FSettings.png)

`Update per second` - allows to define how many times per second the data of an open tab should be updated

# Questions
### How to create a custom drawing method for a type?
To implement your own editor for a specific type or component, you need in the Editor folder of your project  
Create a class that implements `IStaticEcsValueDrawer` or `IStaticEcsValueDrawer<T>`  
method `DrawValue` displays information on tabs `Entity viever`, `Event viever` и `StaticEcsEntityProvider`, `StaticEcsEventProvider`  
method `DrawTableValue` displays information on tabs `Entity table`, `Event table`  

Example:
```csharp
public sealed class IntDrawer : IStaticEcsValueDrawer<int> {
    public override bool DrawValue(ref DrawContext ctx, string label, ref int value) {
        BeginHorizontal();
        PrefixLabel(label);
        var newValue = IntField(GUIContent.none, value);
        EndHorizontal();
        if (newValue == value) {
            return false;
        }

        value = newValue;
        return true;
    }

    public override void DrawTableValue(ref int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
        SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
    }
}
```

### How to use attributes without having a dependency on this module?
It is necessary to copy the attributes by saving the namespace from `\Runtime\Attributes.cs`, after that the attributes will be correctly detected by the editor.


# License
[MIT license](./LICENSE.md)
