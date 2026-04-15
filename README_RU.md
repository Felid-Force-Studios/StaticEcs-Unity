<p align="center">
  <img src="https://raw.githubusercontent.com/Felid-Force-Studios/StaticEcs/master/docs/fulllogo.png" alt="Static ECS" width="100%">
  <br><br>
  <a href="./README.md"><img src="https://img.shields.io/badge/EN-English-blue?style=flat-square" alt="English"></a>
  <a href="./README_RU.md"><img src="https://img.shields.io/badge/RU-Русский-blue?style=flat-square" alt="Русский"></a>
  <a href="./README_ZH.md"><img src="https://img.shields.io/badge/ZH-中文-blue?style=flat-square" alt="中文"></a>
  <br><br>
  <img src="https://img.shields.io/badge/version-2.1.1-blue?style=for-the-badge" alt="Version">
  <a href="https://felid-force-studios.github.io/StaticEcs/ru/"><img src="https://img.shields.io/badge/Docs-документация-blueviolet?style=for-the-badge" alt="Документация"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs"><img src="https://img.shields.io/badge/Core-фреймворк-green?style=for-the-badge" alt="Core фреймворк"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs-Showcase"><img src="https://img.shields.io/badge/Showcase-примеры-yellow?style=for-the-badge" alt="Showcase"></a>
</p>

# Static ECS - C# Entity component system framework - Unity module

## Оглавление
* [Контакты](#контакты)
* [Установка](#установка)
* [Руководство](#руководство)
  * [Подключение](#подключение)
  * [Провайдеры сущностей](#провайдеры-сущностей)
  * [Провайдеры событий](#провайдеры-событий)
  * [Провайдеры Unity-событий](#провайдеры-unity-событий)
  * [Шаблоны](#шаблоны)
  * [Окно просмотра Static ECS](#окно-просмотра-static-ecs)
  * [Настройки](#settings---настройки)
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
        ClientSystems.Create();
        
        EcsDebug<ClientWorldType>.AddWorld<ClientSystemsType>();
        
        ClientWorld.Initialize();
        ClientSystems.Initialize();
```

Для добавления дополнительных групп систем в окно отладки используйте `AddSystem` после инициализации систем:
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
Примечание: `AddWorld` необходимо вызывать до `Initialize` (регистрирует debug-систему), а `AddSystem` — после `Initialize` (системы должны быть уже инициализированы)

### Провайдеры сущностей:  
Скрипт добавляющий возможность конфигурировать сущность в редакторе Unity и автоматически создавать ее в мире ECS  
Добавьте скрипт `StaticEcsEntityProvider` на объект в сцене:

![EntityProvider.png](Readme%2FEntityProvider.png)  

`Usage type` - тип создания, автоматически при вызове `Start()`, `Awake()` или вручную  
`On create type` - действие после создания провайдера, удалить компонент `StaticEcsEntityProvider` с объекта, удалить весь объект или ничего  
`On destroy type` - действие при уничтожении провайдера, уничтожить сущность или ничего  
`Prefab` - позволяет ссылаться на префаб провайдера, при этом изменение данных компонентов будет заблокировано  
`Entity GID` - глобальный идентификатор сущности в рантайме  
`Disable entity on create` - отключает сущность сразу после создания  
`On enable and disable` - включает/отключает сущность при включении/отключении GameObject  
`Entity Type` - тип сущности  
`Cluster ID` - идентификатор кластера в рантайме  
`Components` - данные компонентов  
`Tags` - теги сущности

### Провайдеры событий:
Скрипт добавляющий возможность конфигурировать событие в редакторе Unity и автоматически отправлять его в мир ECS  
Добавьте скрипт `StaticEcsEventProvider` на объект в сцене:

![EventProvider.png](Readme%2FEventProvider.png)  

`Usage type` - тип создания, автоматически при вызове `Start()`, `Awake()` или вручную  
`On create type` - действие после создания, удалить компонент `StaticEcsEventProvider` с объекта, удалить весь объект или ничего  
`World` - тип мира в котором будет отправлено событие  
`Type` - тип события


### Провайдеры Unity-событий:
Система провайдеров для автоматической отправки Unity-событий (физика, GUI) в мир ECS  
Провайдеры перехватывают Unity callbacks (OnCollisionEnter, IPointerClickHandler и т.д.) и отправляют соответствующие ECS события  

#### Регистрация типов

Перед использованием провайдеров необходимо зарегистрировать типы событий и компонентов в мире  
Вызовите `UnityEventTypes.Register<TWorld>()` между `Create()` и `Initialize()`:
```csharp
        ClientWorld.Create(WorldConfig.Default());
        UnityEventTypes.Register<ClientWorldType>(); // Регистрирует все события и компоненты
        ClientWorld.Initialize();
```
Метод автоматически регистрирует все типы событий и компонентов модуля с учётом подключённых модулей (Physics, Physics2D, TextMeshPro)  

Альтернативно, можно использовать `RegisterAll` для автоматической регистрации всех типов из указанных сборок через рефлексию:
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientWorld.Types().RegisterAll(typeof(MyComponent).Assembly); // Все IComponent, IEvent, ITag из сборки
        ClientWorld.Initialize();
```
`RegisterAll` без параметров использует вызывающую сборку. Можно передать несколько сборок  

#### Три режима работы

| Режим | Суффикс класса | Описание |
|---|---|---|
| Без сущности | `Provider<TWorld>` | Отправляет событие в мир ECS без привязки к сущности |
| EntityGID | `EntityGIDProvider<TWorld>` | Привязка к сущности через поле EntityGID |
| EntityRef | `EntityRefProvider<TWorld, TProvider>` | Привязка через ссылку на StaticEcsEntityProvider |

**Режим без сущности** - отправляет событие без EntityGID. Подходит для объектов не связанных с ECS сущностями  

**Режим EntityGID** - хранит EntityGID как сериализуемое поле. Отправляет событие с EntityGID  

**Режим EntityRef** - хранит ссылку на `StaticEcsEntityProvider` компонент. Получает EntityGID из провайдера в рантайме. Удобен когда провайдер сущности находится на том же или соседнем объекте  

#### EntityEventMode

Провайдеры с сущностью (EntityGID/EntityRef) имеют настраиваемое поле `EntityEventMode` в Inspector, определяющее поведение:

| Режим | Описание |
|---|---|
| `All` | Отправляет события **и** управляет компонентами состояния |
| `EventOnly` | Только отправляет ECS события, без управления компонентами (по умолчанию) |
| `ComponentOnly` | Только управляет компонентами состояния на сущности, без отправки событий |

Установка из кода:
```csharp
GetComponent<MyCollision3DEntityGID>().SetEntityEventMode(EntityEventMode.All);
```

#### Генерация провайдеров

Так как Unity не поддерживает сериализацию открытых generic типов, необходимо создать sealed классы-наследники для конкретного мира  

Для быстрой генерации используйте: `Assets/Create/Static ECS/Providers`

Окно генерации позволяет настроить:
- **World** - тип мира для которого генерируются провайдеры
- **Namespace** - пространство имён для генерируемых классов
- **Prefix** - префикс имён классов (по умолчанию имя мира)
- **Event providers** - галочки для выбора категорий событий:
  - GUI - базовые GUI события (Click, Drag, Drop, Pointer, ScrollView, Slider, SubmitCancel, ButtonClick)
  - TextMeshPro - события TMP виджетов (Dropdown, Input)
  - Mouse - события мыши (MouseDownUp, MouseEnterExit, MouseUpAsButton)
  - Physics 3D - события 3D физики (Collision, Trigger, ControllerColliderHit)
  - Physics 2D - события 2D физики (Collision, Trigger)
  - Animation - события анимации (AnimationEvent, StateMachineBehaviour, StateMachineBehaviourLinker)

В результате будут сгенерированы sealed классы готовые к использованию в Unity Inspector  
Для каждого типа события генерируется 3 класса (по одному на каждый режим):
```
{Prefix}Click          : ClickProvider<TWorld>              // без сущности
{Prefix}ClickEntityGID : ClickEntityGIDProvider<TWorld>     // с EntityGID
{Prefix}ClickEntityRef : ClickEntityRefProvider<TWorld, TProvider> // с провайдером
```

#### Поддерживаемые события

**Physics 3D** (требуется `com.unity.modules.physics`):

| Провайдер | Unity Callback | События | Компонент состояния |
|---|---|---|---|
| Collision3D | OnCollisionEnter/Exit | `CollisionEnter3DEvent`, `CollisionExit3DEvent` | `Collision3DState` |
| Trigger3D | OnTriggerEnter/Exit | `TriggerEnter3DEvent`, `TriggerExit3DEvent` | `Trigger3DState` |
| ControllerColliderHit3D | OnControllerColliderHit | `ControllerColliderHit3DEvent` | - |

**Physics 3D — ContactEvent** (требуется `com.unity.modules.physics`, Unity 2022.2+):

Централизованная высокопроизводительная альтернатива MonoBehaviour-коллбэкам на каждом объекте. Использует `Physics.ContactEvent` — batch-callback, получающий все контакты сцены за один вызов. Не требует MonoBehaviour на каждом физическом объекте.

**Архитектура:**
- **Listener** — один на сцену. Подписывается на `Physics.ContactEvent` и обрабатывает все контакты централизованно
- **ContactColliderProvider** — на каждый GameObject с коллайдерами (только для entity-варианта). Регистрирует коллайдеры в маппинге

**Два варианта listener'а:**

| Listener | События | Компонент состояния | Entity-маппинг |
|---|---|---|---|
| `ContactEventListener<TWorld>` | `ContactEnter3DEvent`, `ContactExit3DEvent` | - | Не требуется |
| `ContactEventEntityListener<TWorld>` | `ContactEnter3DEntityEvent`, `ContactExit3DEntityEvent` | `ContactCollision3DState` | Через `ContactColliderEntityMap` |

`ContactEventListener<TWorld>` — отправляет события только с данными коллайдеров, без привязки к сущностям. Один компонент на объекте-менеджере сцены.

`ContactEventEntityListener<TWorld>` — отправляет события с EntityGID обоих тел, используя ресурс мира `ContactColliderEntityMap` для маппинга Collider InstanceID → EntityGID. Один компонент на объекте-менеджере сцены. Опции: `sendNonEntityEvents`, `manageComponents`.

**Регистрация коллайдеров (для entity-варианта):**

`ContactColliderProvider<TWorld>` — MonoBehaviour, который вешается на каждый GameObject с коллайдерами. Регистрирует коллайдеры в `ContactColliderEntityMap` и автоматически устанавливает `providesContacts = true`. Два варианта:
- `ContactColliderGIDProvider<TWorld>` — сериализованный EntityGID
- `ContactColliderRefProvider<TWorld, TProvider>` — ссылка на StaticEcsEntityProvider

Ресурс `ContactColliderEntityMap` создаётся лениво при первой регистрации провайдера.

Контактные события содержат данные обоих коллайдеров:
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

> **Примечание:** `Physics.ContactEvent` сообщает только о реальных столкновениях, не о триггерах. Для триггеров используйте стандартные `Trigger3D` провайдеры.

**Physics 2D** (требуется `com.unity.modules.physics2d`):

| Провайдер | Unity Callback | События | Компонент состояния |
|---|---|---|---|
| Collision2D | OnCollisionEnter2D/Exit2D | `CollisionEnter2DEvent`, `CollisionExit2DEvent` | `Collision2DState` |
| Trigger2D | OnTriggerEnter2D/Exit2D | `TriggerEnter2DEvent`, `TriggerExit2DEvent` | `Trigger2DState` |

**GUI**:

| Провайдер | Unity Interface | События | Компонент состояния |
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

**GUI TMP** (требуется `com.unity.textmeshpro` или `com.unity.ugui` >= 2.0.0):

| Провайдер | Unity Callback | События |
|---|---|---|
| DropdownChange | TMP_Dropdown.onValueChanged | `DropdownChangeEvent` |
| InputChange | TMP_InputField.onValueChanged | `InputChangeEvent` |
| InputEnd | TMP_InputField.onEndEdit | `InputEndEvent` |

**Mouse** (требуется коллайдер на объекте):

| Провайдер | Unity Callback | События | Компонент состояния |
|---|---|---|---|
| MouseDownUp | OnMouseDown/Up | `MouseDownEvent`, `MouseUpEvent` | `MousePressedState` |
| MouseEnterExit | OnMouseEnter/Exit | `MouseEnterEvent`, `MouseExitEvent` | `MouseHoverState` |
| MouseUpAsButton | OnMouseUpAsButton | `MouseUpAsButtonEvent` | - |

**Animation** (требуется `com.unity.modules.animation`):

| Провайдер | Unity Callback | События | Компонент состояния |
|---|---|---|---|
| AnimationEvent | Animation Event (клип) | `AnimationEventEcsEvent` | - |
| StateMachineBehaviour | OnStateEnter/Exit | `AnimatorStateEnterEvent`, `AnimatorStateExitEvent` | - |

`AnimationEventProvider` — размещается на том же GameObject что и Animator. В анимационном клипе укажите имя функции события `OnAnimationEvent`. Событие содержит `stringParameter`, `intParameter`, `floatParameter`, `objectReferenceParameter` и `animationState`.

`StaticEcsStateMachineBehaviour` — добавляется на состояния Animator Controller. Не MonoBehaviour — наследник `StateMachineBehaviour`. Entity-вариант (`StaticEcsEntityStateMachineBehaviour`) хранит сериализованное поле `EntityGID`.

`StaticEcsStateMachineBehaviourLinker` — MonoBehaviour, размещается на том же GameObject что и Animator. Хранит ссылку на `StaticEcsEntityProvider` и в `Start()` автоматически вызывает `SetEntityGID()` на всех `StaticEcsEntityStateMachineBehaviour` найденных в Animator Controller. Имеет публичный метод `Link()` для ручной перепривязки (например после смены сущности в рантайме).

Для режимов с сущностью события имеют суффикс `Entity`: `ClickEntityEvent`, `CollisionEnter3DEntityEvent`, `MouseDownEntityEvent` и т.д.  
Эти события содержат дополнительное поле `EntityGID`

#### Компоненты состояния

Компоненты состояния автоматически управляются провайдерами с сущностью (EntityGID/EntityRef):  
- Добавляются на сущность при Enter-событии
- Удаляются с сущности при Exit-событии
- Обновляются при Move/Update-событии (например `DragState`)

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

#### Валидация

Все провайдеры проверяют `CanSend()` перед отправкой:
- **Базовая проверка**: мир инициализирован (`World<TWorld>.Status == WorldStatus.Initialized`)
- **GUI проверка**: дополнительно проверяет что `Selectable.interactable == true` (если назначен)
- **Entity проверка**: `EntityGID.TryUnpack<TWorld>()` проверяет что сущность жива перед установкой/удалением компонентов

#### Кастомизация

Все методы отправки событий и управления компонентами являются `virtual` и могут быть переопределены:

```csharp
public sealed class MyCollision3D : Collision3DProvider<MyWorldType> {
    protected override bool CanSend() {
        return base.CanSend() && gameObject.activeInHierarchy;
    }

    protected override void OnSendEnterEvent(Collision data) {
        // Кастомная логика вместо или в дополнение к базовой
        base.OnSendEnterEvent(data);
    }
}
```

Для провайдеров с сущностью также доступны:
```csharp
protected override void OnAddComponent(Collision data) { ... }
protected override void OnRemoveComponent() { ... }
```

#### Установка EntityGID / EntityProvider из кода

```csharp
// Для EntityGID провайдеров
GetComponent<MyCollision3DEntityGID>().SetEntityGID(entityGid);

// Для EntityRef провайдеров
GetComponent<MyCollision3DEntityRef>().SetEntityProvider(entityProvider);
```

#### Пример использования

```csharp
using W = World<MyWorldType>;

// 1. Сгенерируйте провайдеры: Assets/Create/Static ECS/Providers
// 2. Добавьте на GameObject нужный провайдер компонент
// 3. Зарегистрируйте приёмники событий и обрабатывайте события в системе:

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

// Или для событий с сущностью:
public struct HandleCollisionSystem : IInitSystem, IUpdateSystem {
    private EventReceiver<MyWorldType, CollisionEnter3DEntityEvent> _receiver;

    public void Init() {
        _receiver = W.RegisterEventReceiver<CollisionEnter3DEntityEvent>();
    }

    public void Update() {
        foreach (var evt in _receiver) {
            if (evt.Value.EntityGID.TryUnpack<MyWorldType>(out var entity)) {
                // Обработка столкновения для конкретной сущности
            }
        }
    }
}

// Или через компоненты состояния (EntityEventMode.ComponentOnly / EntityEventMode.All):
// Компоненты автоматически добавляются при входе и удаляются при выходе
public struct HandleCollisionStateSystem : IUpdateSystem {
    public void Update() {
        foreach (var entity in W.Query<All<Collision3DState>>().Entities()) {
            ref var state = ref entity.Ref<Collision3DState>();
            Debug.Log($"Сущность столкнулась с {state.Collider.name}, скорость: {state.Velocity}");
        }
    }
}
```

> **Примечание:** Компоненты состояния полностью совместимы с [трекингом изменений](https://felid-force-studios.github.io/StaticEcs/ru/features/tracking.html). Компоненты добавляются/удаляются из Unity-коллбеков (FixedUpdate), трекинг-биты записываются в текущий тик. Системы в FixedUpdate и Update увидят эти изменения, если `W.Tick()` вызывается в конце Update. Например, можно использовать `AllAdded<Collision3DState>` для обработки момента начала столкновения:
> ```csharp
> foreach (var entity in W.Query<All<Collision3DState>, AllAdded<Collision3DState>>().Entities()) {
> }
> ```

## Шаблоны:
Генераторы классов доступны в меню создания ассетов `Assets/Create/Static ECS/`

### Providers
Генератор sealed провайдеров для конкретного мира  
Подробнее в разделе [Провайдеры Unity-событий](#провайдеры-unity-событий)

## Окно просмотра Static ECS:
![WindowMenu.png](Readme%2FWindowMenu.png)  

### Entities/Table - таблица просмотра сущностей 

![EntitiesTable.png](Readme%2FEntitiesTable.png)  
 - `Filter` позволяет отобрать необходимые сущности
 - `Entity GID` позволяет найти сущность по глобальному идентификатору
 - `Select` позволяет выбрать отображаемые колонки
 - `Show data` позволяет выбрать отображаемые данные в колонках
 - `Max entities result` максимальное количество отображаемых сущностей

Для отображения данных компонентов в таблице необходимо: установить атрибут `StaticEcsEditorTableValue` в компоненте для field или property
```csharp
public struct Position : IComponent {
    [StaticEcsEditorTableValue]
    public Vector3 Val;
}
```
Для отображения иного имени компонента необходимо установить атрибут `StaticEcsEditorName` в компоненте
```csharp
[StaticEcsEditorName("My velocity")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```
Для установки цвета компонента необходимо установить атрибут `StaticEcsEditorColor` в компоненте (можно установить RGB или HEX color)
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

По умолчанию отображаются только **публичные** поля объектов помеченные атрибутом `[Serializable]`  
- Чтобы отобразить приватное поле, необходимо пометить его атрибутом `[StaticEcsEditorShow]`
- Чтобы скрыть публичное поле, необходимо пометить его атрибутом `[StaticEcsEditorHide]`
- Чтобы запретить редактирование в play mode, необходимо пометить его атрибутом `[StaticEcsEditorRuntimeReadOnly]`
```csharp
public struct SomeComponent : IComponent {
    [StaticEcsEditorShow]
    [StaticEcsEditorRuntimeReadOnly]
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

Для отображения данных компонентов в таблице необходимо установить атрибут `StaticEcsEditorTableValue` в событии для field или property  
```csharp
public struct DamageEvent : IEvent {
    public float Val;

    [StaticEcsEditorTableValue]
    public string ShowData => $"Damage {Val}";
}
```
Для установки цвета событий необходимо установить атрибут `StaticEcsEditorColor` (можно установить RGB или HEX color)
```csharp
[StaticEcsEditorColor("f7796a")]
public struct DamageEvent : IEvent {

}
```
Для игнорирования события в редакторе необходимо установить атрибут `StaticEcsIgnoreEvent`
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

### Settings - настройки
Позволяет настроить поведение окна редактора. Настройки хранятся в ScriptableObject ассете `StaticEcsViewConfig`, который автоматически создается при первом использовании.

- **Config asset** — ссылка на активный конфиг. Можно создать несколько конфигов через `Assets/Create/Static ECS/View Config` и переключаться между ними
- **Component Foldouts** — управление автоматическим раскрытием компонентов на сущностях:
  - `ExpandAll` — все компоненты раскрыты по умолчанию
  - `CollapseAll` — все компоненты свёрнуты по умолчанию
  - `Custom` — только выбранные типы компонентов раскрываются автоматически
- **Reset config to defaults** — сбрасывает все настройки текущего мира на значения по умолчанию

Настройки сохраняются между сессиями. Сохраняется следующее состояние:
- Выбранная вкладка, частота отрисовки
- Таблица сущностей: видимые колонки, колонка сортировки, закрепленные сущности, фильтры, максимальное количество сущностей
- Таблица событий: фильтры типов событий, режим автопрокрутки
- Статистика: порог фрагментации, отображение незарегистрированных
- Системы: максимальная глубина вложенности при отображении свойств систем

Настройки автоматически сохраняются периодически (каждые 30 секунд) и при выходе из Play Mode.

# Вопросы
### Как создать свой метод отрисовки для типа?
Для реализации своего редактора для конкретного типа, создайте `PropertyDrawer` с атрибутом `[CustomPropertyDrawer]` в папке Editor вашего проекта  

Пример:
```csharp
[CustomPropertyDrawer(typeof(MyStruct))]
public class MyStructPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        // Кастомная логика отрисовки
        EditorGUI.EndProperty();
    }
}
```

### Как использовать атрибуты не имея зависимости на данный модуль?
Необходимо скопировать атрибуты сохранив неймспейс из `\Runtime\Attributes.cs`, после этого атрибуты будут корректно обнаружены редактором.


# Лицензия
[MIT license](./LICENSE.md)
