![Version](https://img.shields.io/badge/version-0.9.62-blue.svg?style=for-the-badge)

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
* ### В виде исходников
  Со страцины релизов или как архив из нужной ветки. В ветке `master` стабильная проверенная версия
* ### Установка для Unity
  Как git модуль `https://github.com/Felid-Force-Studios/StaticEcs-Unity.git` в Unity PackageManager или добавления в `Packages/manifest.json`:


# Руководство
Модуль предоставляет дополнительные возможности интеграции с Unity engine:  
### Провайдеры сущностей:  
Скрипт добавляющий возможность конфигурировать сущность в редакторе Unity и автоматически создавать ее в мире ECS  
Добавьте скрипт `StaticEcsEntityProvider` на объект в сцене:

![EntityProvider.png](Readme%2FEntityProvider.png)  

`Usage type` - тип создания, автоматически при вызове Start() MonoBehaviour или вручную  
`On create type` - действие после создания, удалить компонент `StaticEcsEntityProvider` с объекта, удалить весь объект или ничего  
`Wotld` - тип мира в котором будет создана сущность  
`Components` - данные компонентов 
`Tags` - теги сущности  
`Masks` - маски сущности 

### Провайдеры событий:
Скрипт добавляющий возможность конфигурировать событие в редакторе Unity и автоматически отправлять его в мир ECS  
Добавьте скрипт `StaticEcsEventProvider` на объект в сцене:

![EventProvider.png](Readme%2FEventProvider.png)  

`Usage type` - тип создания, автоматически при вызове Start() MonoBehaviour или вручную  
`On create type` - действие после создания, удалить компонент `StaticEcsEntityProvider` с объекта, удалить весь объект или ничего  
`Wotld` - тип мира в котором будет отправлено событие  
`Type` - тип события

### Шаблоны:
Генераторы классов доступны по выбору в меню создания ассетов `Assets/Create/Static ECS/`

## Окно просмотра Static ECS:
Окно мониторинга и управления данными ECS во время выполнения  
Для подключения миров и систем к окну редактора необходимо вызвать специальный метод при инициализации мира и систем  
с указанием требуемого мира или систем
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

### Entities - вкладка просмотра/изменения сущностей в мире  
#### Table - таблица просмотра сущностей 

![EntitiesTable.png](Readme%2FEntitiesTable.png)  
 - `Filter` позволяет отобрать необходимые сущности
 - `Select` позволяет выбрать отображаемые колонки
 - `Show data` позволяет выбрать отображаемые данные в колонках
 - `Max entities result` максимальное количество отображаемых сущностей

Для отображения данных компонентов в таблице необходимо установить аттрибут `StaticEcsEditorTableValue` в компоненте для field или property
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

так же доступны кнопки управления сущностью  
- значок глаза - открыть сущность в просмотрщике
- замок - сделать пин сущности вверху таблицы
- удаление - удалть сущность из мира


#### Viewer -  просмотрщик сущности  
Отображает все данные о сущности с возможностью изменения, добавления и удаления компонентов

![EntitiesViewer.png](Readme%2FEntitiesViewer.png)  

#### Entity builder -  конструктор сущности  
Позволяет настроить и создать новую сущность во время выполнения

![EntitiesBuilder.png](Readme%2FEntitiesBuilder.png)  

### Stats - окно статистики
Отображает все данные о мире, компонентах и событиях

![Stats.png](Readme%2FStats.png)  

### Events - окно просмотра событий 

#### Table -  таблица событий  
Отображает последние события, их данные и количество подписчиков непрочитавших событие

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

#### Viewer -  просмотрщик события
Позволяет просматривать и изменять (только для непрочитанных) данные события

![EventsViewer.png](Readme%2FEventsViewer.png)  

#### Events builder -  конструктор события
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
Для реализации своего редактора для конкретного типа или компонента, необходимо в папке Editor вашего проекта  
Создать класс реализующий `IStaticEcsValueDrawer` или `IStaticEcsValueDrawer<T>`  
метод `DrawValue` отображает информацию на вкладках `Entity viever`, `Event viever` и `StaticEcsEntityProvider`, `StaticEcsEventProvider`  
метод `DrawTableValue` отображает информацию на вкладках `Entity table`, `Event table`  

Пример:
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

# Лицензия
[MIT license](./LICENSE.md)
