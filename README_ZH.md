<p align="center">
  <img src="https://raw.githubusercontent.com/Felid-Force-Studios/StaticEcs/master/docs/fulllogo.png" alt="Static ECS" width="100%">
  <br><br>
  <a href="./README.md"><img src="https://img.shields.io/badge/EN-English-blue?style=flat-square" alt="English"></a>
  <a href="./README_RU.md"><img src="https://img.shields.io/badge/RU-Русский-blue?style=flat-square" alt="Русский"></a>
  <a href="./README_ZH.md"><img src="https://img.shields.io/badge/ZH-中文-blue?style=flat-square" alt="中文"></a>
  <br><br>
  <img src="https://img.shields.io/badge/version-2.1.1-blue?style=for-the-badge" alt="Version">
  <a href="https://felid-force-studios.github.io/StaticEcs/zh/"><img src="https://img.shields.io/badge/Docs-文档-blueviolet?style=for-the-badge" alt="文档"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs"><img src="https://img.shields.io/badge/Core-核心框架-green?style=for-the-badge" alt="核心框架"></a>
  <a href="https://github.com/Felid-Force-Studios/StaticEcs-Showcase"><img src="https://img.shields.io/badge/Showcase-示例-yellow?style=for-the-badge" alt="Showcase"></a>
</p>

# Static ECS - C# Entity component system framework - Unity 模块

## 目录
* [联系方式](#联系方式)
* [安装](#安装)
* [指南](#指南)
  * [连接](#连接)
  * [实体提供者](#实体提供者)
  * [事件提供者](#事件提供者)
  * [Unity 事件提供者](#unity-事件提供者)
  * [模板](#模板)
  * [Static ECS 查看窗口](#static-ecs-查看窗口)
  * [设置](#settings---设置)
* [常见问题](#常见问题)
* [许可证](#许可证)


# 联系方式
* [Telegram](https://t.me/felid_force_studios)

# 安装
还需要安装 [StaticEcs](https://github.com/Felid-Force-Studios/StaticEcs)
* ### 以源代码形式
  从发布页面或从分支下载归档文件。`master` 分支包含稳定测试版本
* ### Unity 安装
  - 通过 Unity PackageManager 的 git 模块     
    `https://github.com/Felid-Force-Studios/StaticEcs-Unity.git`
  - 或添加到 `Packages/manifest.json` 清单文件  
    `"com.felid-force-studios.static-ecs-unity": "https://github.com/Felid-Force-Studios/StaticEcs-Unity.git"`


# 指南
该模块提供与 Unity 引擎的额外集成功能：

### 连接：
ECS 运行时数据监控和管理窗口  
要将世界和系统连接到编辑器窗口，需要在初始化世界和系统时调用特殊方法  
并指定所需的世界或系统
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientSystems.Create();
        
        EcsDebug<ClientWorldType>.AddWorld<ClientSystemsType>();
        
        ClientWorld.Initialize();
        ClientSystems.Initialize();
```

要将额外的系统组添加到调试窗口，请在系统初始化后使用 `AddSystem`：
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
注意：`AddWorld` 必须在 `Initialize` 之前调用（注册调试系统），而 `AddSystem` 必须在 `Initialize` 之后调用（系统必须已初始化）

### 实体提供者：
该脚本添加了在 Unity 编辑器中配置实体并自动在 ECS 世界中创建实体的功能  
将 `StaticEcsEntityProvider` 脚本添加到场景中的对象上：

![EntityProvider.png](Readme%2FEntityProvider.png)

`Usage type` - 创建类型，在 `Start()`、`Awake()` 调用时自动创建，或手动创建  
`On create type` - 创建提供者后的操作，从对象删除 `StaticEcsEntityProvider` 组件、删除整个对象或不做任何操作  
`On destroy type` - 销毁提供者时的操作，销毁实体或不做任何操作  
`Prefab` - 允许引用提供者预制体，同时组件数据的修改将被锁定  
`Entity GID` - 运行时全局实体标识符  
`Disable entity on create` - 创建后立即禁用实体  
`On enable and disable` - 当 GameObject 启用/禁用时启用/禁用实体  
`Entity Type` - 实体类型  
`Cluster ID` - 运行时集群标识符  
`Components` - 组件数据  
`Tags` - 实体标签

### 事件提供者：
该脚本添加了在 Unity 编辑器中配置事件并自动发送到 ECS 世界的功能  
将 `StaticEcsEventProvider` 脚本添加到场景中的对象上：

![EventProvider.png](Readme%2FEventProvider.png)

`Usage type` - 创建类型，在 `Start()`、`Awake()` 调用时自动创建，或手动创建  
`On create type` - 创建后的操作，从对象删除 `StaticEcsEventProvider` 组件、删除整个对象或不做任何操作  
`World` - 发送事件的世界类型  
`Type` - 事件类型


### Unity 事件提供者：
用于自动将 Unity 事件（物理、GUI）转发到 ECS 世界的提供者系统  
提供者拦截 Unity 回调（OnCollisionEnter、IPointerClickHandler 等）并发送对应的 ECS 事件  

#### 类型注册

在使用提供者之前，必须在世界中注册事件和组件类型  
在 `Create()` 和 `Initialize()` 之间调用 `UnityEventTypes.Register<TWorld>()`：
```csharp
        ClientWorld.Create(WorldConfig.Default());
        UnityEventTypes.Register<ClientWorldType>(); // 注册所有事件和组件
        ClientWorld.Initialize();
```
该方法会自动注册所有模块事件和组件类型，并考虑已安装的模块（Physics、Physics2D、TextMeshPro）  

或者，可以使用 `RegisterAll` 通过反射自动注册指定程序集中的所有类型：
```csharp
        ClientWorld.Create(WorldConfig.Default());
        ClientWorld.Types().RegisterAll(typeof(MyComponent).Assembly); // 程序集中的所有 IComponent、IEvent、ITag
        ClientWorld.Initialize();
```
不带参数的 `RegisterAll` 使用调用程序集。可以传入多个程序集  

#### 三种工作模式

| 模式 | 类后缀 | 描述 |
|---|---|---|
| 无实体 | `Provider<TWorld>` | 将事件发送到 ECS 世界，不绑定实体 |
| EntityGID | `EntityGIDProvider<TWorld>` | 通过 EntityGID 字段绑定到实体 |
| EntityRef | `EntityRefProvider<TWorld, TProvider>` | 通过对 StaticEcsEntityProvider 的引用进行绑定 |

**无实体模式** - 发送不带 EntityGID 的事件。适用于与 ECS 实体无关的对象  

**EntityGID 模式** - 将 EntityGID 存储为可序列化字段。发送带有 EntityGID 的事件  

**EntityRef 模式** - 存储对 `StaticEcsEntityProvider` 组件的引用。在运行时从提供者获取 EntityGID。当实体提供者位于相同或相邻对象上时很方便  

#### EntityEventMode

带实体的提供者（EntityGID/EntityRef）在 Inspector 中有可配置的 `EntityEventMode` 字段，用于控制行为：

| 模式 | 描述 |
|---|---|
| `All` | 发送事件**并**管理状态组件 |
| `EventOnly` | 仅发送 ECS 事件，不管理组件（默认） |
| `ComponentOnly` | 仅管理实体上的状态组件，不发送事件 |

从代码设置：
```csharp
GetComponent<MyCollision3DEntityGID>().SetEntityEventMode(EntityEventMode.All);
```

#### 生成提供者

由于 Unity 不支持开放泛型类型的序列化，需要为特定世界创建 sealed 子类  

使用快速生成：`Assets/Create/Static ECS/Providers`

生成窗口允许配置：
- **World** - 生成提供者的世界类型
- **Namespace** - 生成类的命名空间
- **Prefix** - 类名前缀（默认为世界名称）
- **Event providers** - 选择事件类别的复选框：
  - GUI - 基础 GUI 事件（Click、Drag、Drop、Pointer、ScrollView、Slider、SubmitCancel、ButtonClick）
  - TextMeshPro - TMP 控件事件（Dropdown、Input）
  - Mouse - 鼠标事件（MouseDownUp、MouseEnterExit、MouseUpAsButton）
  - Physics 3D - 3D 物理事件（Collision、Trigger、ControllerColliderHit）
  - Physics 2D - 2D 物理事件（Collision、Trigger）
  - Animation - 动画事件（AnimationEvent、StateMachineBehaviour、StateMachineBehaviourLinker）

结果将生成可在 Unity Inspector 中使用的 sealed 类  
每种事件类型生成 3 个类（每种模式一个）：
```
{Prefix}Click          : ClickProvider<TWorld>              // 无实体
{Prefix}ClickEntityGID : ClickEntityGIDProvider<TWorld>     // 带 EntityGID
{Prefix}ClickEntityRef : ClickEntityRefProvider<TWorld, TProvider> // 带提供者
```

#### 支持的事件

**Physics 3D**（需要 `com.unity.modules.physics`）：

| 提供者 | Unity 回调 | 事件 | 状态组件 |
|---|---|---|---|
| Collision3D | OnCollisionEnter/Exit | `CollisionEnter3DEvent`, `CollisionExit3DEvent` | `Collision3DState` |
| Trigger3D | OnTriggerEnter/Exit | `TriggerEnter3DEvent`, `TriggerExit3DEvent` | `Trigger3DState` |
| ControllerColliderHit3D | OnControllerColliderHit | `ControllerColliderHit3DEvent` | - |

**Physics 3D — ContactEvent**（需要 `com.unity.modules.physics`，Unity 2022.2+）：

集中式高性能替代方案，取代每个对象上的 MonoBehaviour 回调。使用 `Physics.ContactEvent` — 批量回调，在一次调用中接收场景中的所有接触。不需要在每个物理对象上添加 MonoBehaviour。

**架构：**
- **Listener** — 每个场景一个。订阅 `Physics.ContactEvent` 并集中处理所有接触
- **ContactColliderProvider** — 在每个带碰撞体的 GameObject 上（仅限实体变体）。在映射中注册碰撞体

**两种 Listener 变体：**

| Listener | 事件 | 状态组件 | 实体映射 |
|---|---|---|---|
| `ContactEventListener<TWorld>` | `ContactEnter3DEvent`, `ContactExit3DEvent` | - | 不需要 |
| `ContactEventEntityListener<TWorld>` | `ContactEnter3DEntityEvent`, `ContactExit3DEntityEvent` | `ContactCollision3DState` | 通过 `ContactColliderEntityMap` |

`ContactEventListener<TWorld>` — 仅发送包含碰撞体数据的事件，不绑定实体。一个组件放在场景管理器对象上。

`ContactEventEntityListener<TWorld>` — 发送包含双方 EntityGID 的事件，使用世界资源 `ContactColliderEntityMap` 进行 Collider InstanceID → EntityGID 映射。一个组件放在场景管理器对象上。选项：`sendNonEntityEvents`、`manageComponents`。

**碰撞体注册（仅限实体变体）：**

`ContactColliderProvider<TWorld>` — 放置在每个带碰撞体的 GameObject 上的 MonoBehaviour。在 `ContactColliderEntityMap` 中注册碰撞体并自动设置 `providesContacts = true`。两种变体：
- `ContactColliderGIDProvider<TWorld>` — 序列化的 EntityGID
- `ContactColliderRefProvider<TWorld, TProvider>` — 对 StaticEcsEntityProvider 的引用

`ContactColliderEntityMap` 资源在第一个提供者注册时惰性创建。

接触事件包含双方碰撞体的数据：
```csharp
public struct ContactEnter3DEntityEvent : IEvent {
    public EntityGID EntityA;
    public EntityGID EntityB;
    public Collider ColliderA;
    public Collider ColliderB;
    public Vector3 Point;
    public Vector3 Normal;
    public float Impulse;
}
```

> **注意：** `Physics.ContactEvent` 仅报告实际碰撞，不报告触发器。对于触发器，请使用标准的 `Trigger3D` 提供者。

**Physics 2D**（需要 `com.unity.modules.physics2d`）：

| 提供者 | Unity 回调 | 事件 | 状态组件 |
|---|---|---|---|
| Collision2D | OnCollisionEnter2D/Exit2D | `CollisionEnter2DEvent`, `CollisionExit2DEvent` | `Collision2DState` |
| Trigger2D | OnTriggerEnter2D/Exit2D | `TriggerEnter2DEvent`, `TriggerExit2DEvent` | `Trigger2DState` |

**GUI**：

| 提供者 | Unity 接口 | 事件 | 状态组件 |
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

**GUI TMP**（需要 `com.unity.textmeshpro` 或 `com.unity.ugui` >= 2.0.0）：

| 提供者 | Unity 回调 | 事件 |
|---|---|---|
| DropdownChange | TMP_Dropdown.onValueChanged | `DropdownChangeEvent` |
| InputChange | TMP_InputField.onValueChanged | `InputChangeEvent` |
| InputEnd | TMP_InputField.onEndEdit | `InputEndEvent` |

**Mouse**（需要对象上有碰撞体）：

| 提供者 | Unity 回调 | 事件 | 状态组件 |
|---|---|---|---|
| MouseDownUp | OnMouseDown/Up | `MouseDownEvent`, `MouseUpEvent` | `MousePressedState` |
| MouseEnterExit | OnMouseEnter/Exit | `MouseEnterEvent`, `MouseExitEvent` | `MouseHoverState` |
| MouseUpAsButton | OnMouseUpAsButton | `MouseUpAsButtonEvent` | - |

**Animation**（需要 `com.unity.modules.animation`）：

| 提供者 | Unity 回调 | 事件 | 状态组件 |
|---|---|---|---|
| AnimationEvent | Animation Event（动画剪辑） | `AnimationEventEcsEvent` | - |
| StateMachineBehaviour | OnStateEnter/Exit | `AnimatorStateEnterEvent`, `AnimatorStateExitEvent` | - |

`AnimationEventProvider` — 放置在与 Animator 相同的 GameObject 上。在动画剪辑中将事件函数名设置为 `OnAnimationEvent`。事件包含 `stringParameter`、`intParameter`、`floatParameter`、`objectReferenceParameter` 和 `animationState`。

`StaticEcsStateMachineBehaviour` — 添加到 Animator Controller 状态上。非 MonoBehaviour — 继承自 `StateMachineBehaviour`。实体变体（`StaticEcsEntityStateMachineBehaviour`）存储序列化的 `EntityGID` 字段。

`StaticEcsStateMachineBehaviourLinker` — MonoBehaviour，放置在与 Animator 相同的 GameObject 上。存储对 `StaticEcsEntityProvider` 的引用，在 `Start()` 中自动对 Animator Controller 中找到的所有 `StaticEcsEntityStateMachineBehaviour` 调用 `SetEntityGID()`。提供公共 `Link()` 方法用于手动重新链接（例如在运行时更换实体后）。

对于带实体的模式，事件带有 `Entity` 后缀：`ClickEntityEvent`、`CollisionEnter3DEntityEvent`、`MouseDownEntityEvent` 等。  
这些事件包含额外的 `EntityGID` 字段

#### 状态组件

状态组件由带实体的提供者（EntityGID/EntityRef）自动管理：  
- 在 Enter 事件时添加到实体
- 在 Exit 事件时从实体移除
- 在 Move/Update 事件时更新（例如 `DragState`）

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

#### 验证

所有提供者在发送前检查 `CanSend()`：
- **基础检查**：世界已初始化（`World<TWorld>.Status == WorldStatus.Initialized`）
- **GUI 检查**：额外检查 `Selectable.interactable == true`（如果已分配）
- **实体检查**：`EntityGID.TryUnpack<TWorld>()` 在设置/删除组件前验证实体是否存活

#### 自定义

所有事件发送和组件管理方法都是 `virtual` 的，可以被重写：

```csharp
public sealed class MyCollision3D : Collision3DProvider<MyWorldType> {
    protected override bool CanSend() {
        return base.CanSend() && gameObject.activeInHierarchy;
    }

    protected override void OnSendEnterEvent(Collision data) {
        // 自定义逻辑，替代或补充基础逻辑
        base.OnSendEnterEvent(data);
    }
}
```

对于带实体的提供者，还可以重写：
```csharp
protected override void OnAddComponent(Collision data) { ... }
protected override void OnRemoveComponent() { ... }
```

#### 从代码设置 EntityGID / EntityProvider

```csharp
// 对于 EntityGID 提供者
GetComponent<MyCollision3DEntityGID>().SetEntityGID(entityGid);

// 对于 EntityRef 提供者
GetComponent<MyCollision3DEntityRef>().SetEntityProvider(entityProvider);
```

#### 使用示例

```csharp
using W = World<MyWorldType>;

// 1. 生成提供者：Assets/Create/Static ECS/Providers
// 2. 将所需的提供者组件添加到 GameObject
// 3. 注册事件接收器并在系统中处理事件：

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

// 或处理带实体的事件：
public struct HandleCollisionSystem : IInitSystem, IUpdateSystem {
    private EventReceiver<MyWorldType, CollisionEnter3DEntityEvent> _receiver;

    public void Init() {
        _receiver = W.RegisterEventReceiver<CollisionEnter3DEntityEvent>();
    }

    public void Update() {
        foreach (var evt in _receiver) {
            if (evt.Value.EntityGID.TryUnpack<MyWorldType>(out var entity)) {
                // 处理特定实体的碰撞
            }
        }
    }
}

// 或使用状态组件（EntityEventMode.ComponentOnly / EntityEventMode.All）：
// 组件在进入时自动添加，在退出时自动移除
public struct HandleCollisionStateSystem : IUpdateSystem {
    public void Update() {
        foreach (var entity in W.Query<All<Collision3DState>>().Entities()) {
            ref var state = ref entity.Ref<Collision3DState>();
            Debug.Log($"实体正在与 {state.Collider.name} 碰撞，速度: {state.Velocity}");
        }
    }
}
```

> **注意：** 状态组件完全兼容[变更追踪](https://felid-force-studios.github.io/StaticEcs/zh/features/tracking.html)。组件从 Unity 回调（FixedUpdate）中添加/移除，追踪位写入当前 tick。只要在 Update 结束时调用 `W.Tick()`，FixedUpdate 和 Update 中的系统都能看到这些变更。例如，可以使用 `AllAdded<Collision3DState>` 检测碰撞开始的时刻：
> ```csharp
> foreach (var entity in W.Query<All<Collision3DState>, AllAdded<Collision3DState>>().Entities()) {
> }
> ```

## 模板：
类生成器可在资源创建菜单 `Assets/Create/Static ECS/` 中使用

### Providers
为特定世界生成 sealed 提供者  
详见 [Unity 事件提供者](#unity-事件提供者) 部分

## Static ECS 查看窗口：
![WindowMenu.png](Readme%2FWindowMenu.png)

### Entities/Table - 实体查看表

![EntitiesTable.png](Readme%2FEntitiesTable.png)
- `Filter` 允许筛选所需的实体
- `Entity GID` 允许通过全局标识符查找实体
- `Select` 允许选择要显示的列
- `Show data` 允许选择列中显示的数据
- `Max entities result` 显示的最大实体数量

要在表中显示组件数据，需要在组件的字段或属性上设置 `StaticEcsEditorTableValue` 特性
```csharp
public struct Position : IComponent {
    [StaticEcsEditorTableValue]
    public Vector3 Val;
}
```
要显示不同的组件名称，需要在组件上设置 `StaticEcsEditorName` 特性
```csharp
[StaticEcsEditorName("My velocity")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```
要设置组件颜色，需要在组件上设置 `StaticEcsEditorColor` 特性（可以设置 RGB 或 HEX 颜色）
```csharp
[StaticEcsEditorColor("f7796a")]
public struct Velocity : IComponent {
    [StaticEcsEditorTableValue]
    public float Val;
}
```

还提供实体控制按钮  
- 眼睛图标 - 打开实体进行查看
- 锁定 - 在表中锁定实体
- 删除 - 在世界中销毁实体


### Viewer - 实体查看窗口
显示实体的所有数据，可修改、添加和删除组件

![EntitiesViewer.png](Readme%2FEntitiesViewer.png)

默认情况下，仅显示标有 `[Serializable]` 特性的**公共**字段
- 要显示私有字段，需要用 `[StaticEcsEditorShow]` 特性标记
- 要隐藏公共字段，需要用 `[StaticEcsEditorHide]` 特性标记
- 要在运行模式下禁止编辑，可以用 `[StaticEcsEditorRuntimeReadOnly]` 特性标记
```csharp
public struct SomeComponent : IComponent {
    [StaticEcsEditorShow]
    [StaticEcsEditorRuntimeReadOnly]
    private int _showData;
    
    [StaticEcsEditorHide]
    public int HideData;
}
```

### Entities/Builder - 实体构建器
允许在运行时配置和创建新实体（类似于实体提供者）

![EntitiesBuilder.png](Readme%2FEntitiesBuilder.png)

### Stats - 统计窗口
显示所有世界、组件和事件数据

![Stats.png](Readme%2FStats.png)

### Events/Table - 事件表
显示最近的事件、其详细信息和未读事件的订阅者数量

![EventsTable.png](Readme%2FEventsTable.png)

黄色标记的事件表示已被明确抑制  
灰色标记的事件表示已被所有订阅者读取

要在表中显示组件数据，需要在事件的字段或属性上设置 `StaticEcsEditorTableValue` 特性
```csharp
public struct DamageEvent : IEvent {
    public float Val;

    [StaticEcsEditorTableValue]
    public string ShowData => $"Damage {Val}";
}
```
要设置事件颜色，需要设置 `StaticEcsEditorColor` 特性（可以设置 RGB 或 HEX 颜色）
```csharp
[StaticEcsEditorColor("f7796a")]
public struct DamageEvent : IEvent {

}
```
要在编辑器中忽略事件，需要设置 `StaticEcsIgnoreEvent` 特性
```csharp
[StaticEcsIgnoreEvent]
public struct DamageEvent : IEvent {

}
```

### Viewer - 事件查看窗口
允许查看和修改（仅限未读）事件数据

![EventsViewer.png](Readme%2FEventsViewer.png)

### Events/Builder - 事件构建器
允许在运行时配置和创建新事件

![EventsBuilder.png](Readme%2FEventsBuilder.png)


### Systems - 系统窗口
按执行顺序显示所有系统  
允许在运行时启用和禁用系统  
显示每个系统的平均执行时间  

![Systems.png](Readme%2FSystems.png)

### Settings - 设置
允许配置编辑器窗口行为。设置存储在 `StaticEcsViewConfig` ScriptableObject 资产中，首次使用时自动创建。

- **Config asset** — 当前活动配置的引用。可以通过 `Assets/Create/Static ECS/View Config` 创建多个配置并在它们之间切换
- **Component Foldouts** — 控制实体上组件折叠的自动展开：
  - `ExpandAll` — 默认展开所有组件
  - `CollapseAll` — 默认折叠所有组件
  - `Custom` — 仅自动展开选定的组件类型
- **Reset config to defaults** — 将当前世界的所有设置重置为默认值

设置在会话之间持久保存。保存以下状态：
- 选定的标签页、绘制频率
- 实体表：可见列、排序列、固定实体、过滤器、最大实体数量
- 事件表：事件类型过滤器、自动滚动模式
- 统计：碎片化阈值、显示未注册切换
- 系统：系统属性显示的最大嵌套深度

设置每30秒自动保存一次，退出 Play Mode 时也会保存。

# 常见问题
### 如何为类型创建自定义绘制方法？
要为特定类型实现自定义编辑器，在项目的 Editor 文件夹中创建带有 `[CustomPropertyDrawer]` 特性的 `PropertyDrawer`  

示例：
```csharp
[CustomPropertyDrawer(typeof(MyStruct))]
public class MyStructPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        // 自定义绘制逻辑
        EditorGUI.EndProperty();
    }
}
```

### 如何在不依赖此模块的情况下使用特性？
需要从 `\Runtime\Attributes.cs` 中复制特性并保留命名空间，之后特性将被编辑器正确检测。


# 许可证
[MIT license](./LICENSE.md)
