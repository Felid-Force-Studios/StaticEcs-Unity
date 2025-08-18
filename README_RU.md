![Version](https://img.shields.io/badge/version-1.0.25-blue.svg?style=for-the-badge)

### ЯЗЫК
[RU](./README_RU.md)
[EN](./README.md)
___

### [Static ECS](https://github.com/Felid-Force-Studios/StaticEcs)  

# Static ECS - C# Entity component system framework - Unity module

### Ограничения и особенности:
> - Не потокобезопасен
> - Могут быть незначительные изменения API

## Оглавление
* [Контакты](#контакты)
* [Установка](#установка)
* [Руководство](#руководство)
* [Вопросы](#вопросы)
* [Лицензия](#лицензия)


# Контакты
* [Telegram](https://t.me/felid_force_studios)
# Установка
Должен быть так же установлен [StaticEcs](https://github.com/Felid-Force-Studios/StaticEcs)
* ### В виде исходников
  Со страницы релизов или как архив из нужной ветки. В ветке `master` стабильная проверенная версия
* ### Установка для Unity
  - Как git модуль в Unity PackageManager     
    `https://github.com/Felid-Force-Studios/StaticEcs-Unity.git`  
  - Или добавление в манифест `Packages/manifest.json`  
    `"com.felid-force-studios.static-ecs-unity": "https://github.com/Felid-Force-Studios/StaticEcs-Unity.git"`  


# Руководство
Модуль предоставляет дополнительные возможности интеграции с Unity engine:  

### Подключение:
Окно мониторинга и управления данными ECS во время выполнения  
Для подключения миров и систем к окну редактора необходимо вызвать специальный метод при инициализации мира и систем  
с указанием требуемого мира или систем
```csharp
        ClientWorld.Create(WorldConfig.Default());
        //...
        EcsDebug<ClientWorldType>.AddWorld(); // Между созданием и инициализацией
        //...
        ClientWorld.Initialize();
        
        ClientSystems.Create();
        //...
        ClientSystems.Initialize();
        EcsDebug<ClientWorldType>.AddSystem<ClientSystemsType>(); // После инициализации
```

### Провайдеры сущностей:  
Скрипт добавляющий возможность конфигурировать сущность в редакторе Unity и автоматически создавать ее в мире ECS  
Добавьте скрипт `StaticEcsEntityProvider` на объект в сцене:

![EntityProvider.png](Readme%2FEntityProvider.png)  

`Usage type` - тип создания, автоматически при вызове `Start()`, `Awake()` или вручную  
`On create type` - действие после создания провайдера, удалить компонент `StaticEcsEntityProvider` с объекта, удалить весь объект или ничего  
`On destroy type` - действие при уничтожении провайдера, уничтожить сущность или ничего  
`Prefab` - позволяет ссылаться на префаб провайдера, при этом изменение данных компонентов будет заблокировано  
`Wotld` - тип мира в котором будет создана сущность  
`Entity ID` - идентификатор сущности в рантайме  
`Entity GID` - глобальный идентификатор сущности в рантайме  
`Standard components` - данные компонентов 
`Components` - данные компонентов 
`Tags` - теги сущности  
`Masks` - маски сущности 

### Провайдеры событий:
Скрипт добавляющий возможность конфигурировать событие в редакторе Unity и автоматически отправлять его в мир ECS  
Добавьте скрипт `StaticEcsEventProvider` на объект в сцене:

![EventProvider.png](Readme%2FEventProvider.png)  

`Usage type` - тип создания, автоматически при вызове `Start()`, `Awake()` или вручную  
`On create type` - действие после создания, удалить компонент `StaticEcsEventProvider` с объекта, удалить весь объект или ничего  
`Wotld` - тип мира в котором будет отправлено событие  
`Type` - тип события

### Шаблоны:
Генераторы классов доступны по выбору в меню создания ассетов `Assets/Create/Static ECS/`

## Окно просмотра Static ECS:
![WindowMenu.png](Readme%2FWindowMenu.png)  

### Entities/Table - таблица просмотра сущностей 

![EntitiesTable.png](Readme%2FEntitiesTable.png)  
 - `Filter` позволяет отобрать необходимые сущности
 - `Entity GID` позволяет найти сущность по глобальному идентификатору
 - `Select` позволяет выбрать отображаемые колонки
 - `Show data` позволяет выбрать отображаемые данные в колонках
 - `Max entities result` максимальное количество отображаемых сущностей

Для отображения данных компонентов в таблице необходимо: установить аттрибут `StaticEcsEditorTableValue` в компоненте для field или property
```csharp
public struct Position : IComponent {
    [StaticEcsEditorTableValue]
    public Vector3 Val;
}
```
Для отображения иного имени компонента необходимо установить аттрибут `StaticEcsEditorName` в компоненте
```csharp
[StaticEcsEditorName("My velocity")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```
Для установки цвета компонента необходимо установить аттрибут `StaticEcsEditorColor` в компоненте (можно установить RGB или HEX color)
```csharp
[StaticEcsEditorColor("f7796a")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```

так же доступны кнопки управления сущностью  
- значок глаза - открыть сущность на просмотр
- замок - зафиксировать сущность в таблице
- удаление - уничтожить сущность в мире


### Viewer -  окно просмотра сущности  
Отображает все данные о сущности с возможностью изменения, добавления и удаления компонентов

![EntitiesViewer.png](Readme%2FEntitiesViewer.png)  

По умолчанию отображаются только **публичные** поля объектов помеченные аттрибутом `[Serializable]`  
- Чтобы отобразить приватное поле, необходимо пометить его аттрибутом `[StaticEcsEditorShow]`
- Чтобы скрыть публичное поле, необходимо пометить его аттрибутом `[StaticEcsEditorHide]`
```csharp
public struct SomeComponent : IComponent {
    [StaticEcsEditorShow]
    private int _showData;
    
    [StaticEcsEditorHide]
    public int HideData;
}
```

### Entities/Builder -  конструктор сущности  
Позволяет настроить и создать новую сущность во время выполнения (Аналогичен провайдеру сущностей)

![EntitiesBuilder.png](Readme%2FEntitiesBuilder.png)  

### Stats - окно статистики
Отображает все данные о мире, компонентах и событиях

![Stats.png](Readme%2FStats.png)  

### Events/Table -  таблица событий  
Отображает последние события, их данные и количество подписчиков не прочитавших событие

![EventsTable.png](Readme%2FEventsTable.png)  

События помеченные желтым цветом означают что их явно подавили  
События помеченные серым цветом означают что их прочитали все подписчики  

Для отображения данных компонентов в таблице необходимо установить аттрибут `StaticEcsEditorTableValue` в событии для field или property  
```csharp
public struct DamageEvent : IEvent {
    public float Val;

    [StaticEcsEditorTableValue]
    public string ShowData => $"Damage {Val}";
}
```
Для установки цвета событий необходимо установить аттрибут `StaticEcsEditorColor` (можно установить RGB или HEX color)
```csharp
[StaticEcsEditorColor("f7796a")]
public struct DamageEvent : IEvent {

}
```
Для игнорирования события в редакторе необходимо установить аттрибут `StaticEcsIgnoreEvent`
```csharp
[StaticEcsIgnoreEvent]
public struct DamageEvent : IEvent {

}
```

### Viewer - окно просмотра событий
Позволяет просматривать и изменять (только для непрочитанных) данные события

![EventsViewer.png](Readme%2FEventsViewer.png)  

### Events/Builder -  конструктор события
Позволяет настроить и создать новое событие во время выполнения  

![EventsBuilder.png](Readme%2FEventsBuilder.png)  


### Systems - окно систем 
Отображает все системы в том порядке в котором они выполняются  
Позволяет включать и отключать системы во время выполнения  
Отображает среднее время выполнения каждой системы  

![Systems.png](Readme%2FSystems.png)  

### Settings - настройки редактора

![Settings.png](Readme%2FSettings.png)  

`Update per second` - позволяет определить сколько раз в секунду требуется обновлять данные открытой вкладки

# Вопросы
### Как создать свой метод отрисовки для типа?  
Для реализации своего редактора для конкретного типа или компонента, необходимо в папке Editor вашего проекта  
Создать класс реализующий `IStaticEcsValueDrawer` или `IStaticEcsValueDrawer<T>`  
метод `DrawValue` отображает информацию на вкладках `Entity viever`, `Event viever` и `StaticEcsEntityProvider`, `StaticEcsEventProvider`  
метод `DrawTableValue` отображает информацию на вкладках `Entity table`, `Event table`  

Пример:
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

### Как использовать аттрибуты не имея зависимости на данный модуль?
Необходимо скопировать аттрибуты сохранив неймспейс из `\Runtime\Attributes.cs`, после этого аттрибуты будут корректно обнаружены редактором.


# Лицензия
[MIT license](./LICENSE.md)
