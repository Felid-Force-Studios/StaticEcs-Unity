![Version](https://img.shields.io/badge/version-0.9.30-blue.svg?style=for-the-badge)

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
* ### As source code
  From the release page or as an archive from the branch. In the `master` branch there is a stable tested version
* ### Installation for Unity
  git module `https://github.com/Felid-Force-Studios/StaticEcs-Unity.git` in Unity PackageManager or adding it to `Packages/manifest.json`


# Guide
The module provides additional integration options with the Unity engine:
### Entity Providers:
A script that adds the option to configure an entity in the Unity editor and automatically create it in the ECS world  
Add the `StaticEcsEntityProvider` script to an object in the scene:

![EntityProvider.png](Readme%2FEntityProvider.png)

`Usage type` - type of creation, automatically when calling Start() MonoBehaviour or manually  
`On create type` - action after creation, delete the `StaticEcsEntityProvider` component from the object, delete the entire object, or nothing   
`Wotld` - the type of world in which the entity will be created  
`Components` - component data
`Tags` - entity tags  
`Masks` - entity masks

### Event providers:
A script that adds the ability to configure an event in the Unity editor and automatically send it to the ECS world    
Add the `StaticEcsEventProvider` script to an object in the scene:

![EventProvider.png](Readme%2FEventProvider.png)

`Usage type` - type of creation, automatically when calling Start() MonoBehaviour or manually    
`On create type` - action after creation, delete the `StaticEcsEntityProvider` component from the object, delete the entire object, or nothing    
`Wotld` - type of world in which the event will be sent  
`Type` - event type

### Templates:
Class generators are optionally available in the Asset Creation menu `Assets/Create/Static ECS/`

## Static ECS view window:
ECS runtime data monitoring and management window    
To connect worlds and systems to the editor window, you must call a special method when initializing the world and systems    
specifying the world or systems required
```csharp
        ClientEcs.Create(EcsConfig.Default());
        //...
        EcsDebug<ClientWorldType>.AddWorld();
        //...
        ClientEcs.Initialize();
        
        ClientSystems.Create();
        //...
        EcsDebug<ClientWorldType>.AddSystem<ClientSystemsType>();
        //...
        ClientSystems.Initialize();
```

![WindowMenu.png](Readme%2FWindowMenu.png)

### Entities - tab for viewing/modifying entities in the world
#### Table

![EntitiesTable.png](Readme%2FEntitiesTable.png)
- `Filter` allows to select the necessary entities
- `Select` allows you to select the columns to display
- `Show data` allows to select the displayed data in the columns
- `Max entities result` maximum number of displayed entities

To display component data in a table, you must set the `StaticEcsEditorTableValue` attribute in the component to field or property
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

entity control buttons are also available
- eye icon - open the entity in the viewer
- lock - pin the entity to the top of the table
- trash - destroy the entity from the world


#### Entity viewer
Displays all entity data with the option to modify, add and delete components

![EntitiesViewer.png](Readme%2FEntitiesViewer.png)

#### Entity builder
Allows customization and creation of a new entity at runtime

![EntitiesBuilder.png](Readme%2FEntitiesBuilder.png)

### Stats
Displays all world, component and event data

![Stats.png](Readme%2FStats.png)

### Events

#### Table
Displays recent events, their details and the number of subscribers who have unread the event

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

#### Event viewer
Allow the viewing and modification (for unread only) of event data

![EventsViewer.png](Readme%2FEventsViewer.png)

#### Events builder
Allows to configure and create a new event at runtime

![EventsBuilder.png](Readme%2FEventsBuilder.png)


### Systems
Displays all systems in the order in which they are executed  
Allows turning systems on and off during runtime    
Displays the average execution time of each system  

![Systems.png](Readme%2FSystems.png)

### Settings

![Settings.png](Readme%2FSettings.png)

`Update per second` - allows to define how many times per second the data of an open tab should be updated

# Questions
To implement your own editor for a specific type or component, you need in the Editor folder of your project  
Create a class that implements `IStaticEcsValueDrawer` or `IStaticEcsValueDrawer<T>`  
method `DrawValue` displays information on tabs `Entity viever`, `Event viever` и `StaticEcsEntityProvider`, `StaticEcsEventProvider`  
method `DrawTableValue` displays information on tabs `Entity table`, `Event table`  

Example:
```csharp
    sealed class IntDrawer : IStaticEcsValueDrawer<int> {
        protected override bool DrawValue(string label, ref int value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
```

# License
[MIT license](./LICENSE.md)
