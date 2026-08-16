# Endstone.DotNet Loader API 参考

以下每个页面都是**从源码生成**的:代码生成器(`tools/DocGen`)对 `Endstone.Loader.dll` 做反射,合并 `src/csharp` 中的 XML 文档注释,然后渲染成 这些 Markdown 页面。请勿手工编辑,运行生成器即可。

## 核心

| 类型 | 说明 |
| --- | --- |
| [`Server`](Server.md) | 包装原生 endstone::Server 单例的托管对象,是访问服务器信息、广播、玩家、地图、boss 血条与世界的入口。 |
| [`PluginBase`](PluginBase.md) | 所有 .NET 插件的基类。继承并在主类上添加 [Plugin] 特性;重写 OnLoad/OnEnable/OnDisable 生命周期方法,通过 Command()/RegisterEvent()/Scheduler/ServiceManager 使用服务器功能。 |
| [`PluginAttribute`](PluginAttribute.md) | 插件声明特性:标记插件主类,并提供名称、版本、描述与作者信息。 |
| [`Logger`](Logger.md) | 插件日志记录器,输出到服务器的插件日志。Trace 级别默认被过滤,Info 及以上可见。 |
| [`Scheduler`](Scheduler.md) | 插件作用域的调度器外壳,封装原生 endstone Scheduler。任务归属于插件,插件停用时自动全部取消。 |
| [`ScheduledTask`](ScheduledTask.md) | 排队到原生 endstone 调度器的任务句柄。同步任务在服务器线程执行,异步任务在工作线程执行。 |
| [`Service`](Service.md) | 与服务器的服务管理器注册的服务的基类(对应 endstone::Service)。服务是标记式接口:提供者继承本类并实现自己的方法即可。 |
| [`ServiceManager`](ServiceManager.md) | 插件作用域的服务管理器外壳(对应 endstone::ServiceManager)。注册的提供者关联到本插件,插件停用时自动注销。 |
| [`CommandSender`](CommandSender.md) | 命令发送者的抽象,既可以是玩家也可以是控制台。通过 AsPlayer() 可判断并转型。 |
| [`CommandBuilder`](CommandBuilder.md) | 命令的流式构建器:配置描述、用法、别名、权限,并绑定命令处理函数。 |

## 实体

| 类型 | 说明 |
| --- | --- |
| [`Actor`](Actor.md) | 包装原生 endstone::Actor 的托管对象。实体是世界中可移动、可交互的对象(玩家、生物、掉落物等)。 |
| [`Player`](Player.md) | 包装原生 endstone::Player 的托管对象(继承 Actor),提供消息、操作、物品、模式、飞行、传送等玩家功能。 |
| [`Mob`](Mob.md) | 包装原生 endstone::Mob 的托管对象(继承 Actor),提供生命值等生物专属属性。 |
| [`DamageSource`](DamageSource.md) | 伤害来源信息:伤害类型、直接/间接伤害者。 |
| [`Enchantment`](Enchantment.md) | 包装原生 endstone::Enchantment 的托管对象。通过 Enchantment.Get(键) 或静态常量(如 Sharpness)获取,并提供常用附魔的静态属性。 |
| [`ItemEnchantment`](ItemEnchantment.md) | 物品上的一条附魔记录(附魔 + 等级)。 |

## 世界

| 类型 | 说明 |
| --- | --- |
| [`Level`](Level.md) | 包装原生 endstone::Level 的托管对象,表示服务器世界。可读写时间、获取种子、查询维度与实体。 |
| [`Dimension`](Dimension.md) | 包装原生 endstone::Dimension 的托管对象,表示一个维度(主世界、下界、末地等)。 |
| [`Chunk`](Chunk.md) | 包装原生 endstone::Chunk 的托管对象,表示一个区块 (16 x 16 x 384)。 |
| [`Block`](Block.md) | 包装原生 endstone::Block 的托管对象,表示世界中的方块实例。 |
| [`BlockState`](BlockState.md) | 方块状态的快照,可独立修改再更新到世界中,用于实现原子化的方块改动。 |
| [`Location`](Location.md) | 三维坐标 + 视角(yaw/pitch)的值类型,用于表示实体/方块/生成位置。 |

## 物品与背包

| 类型 | 说明 |
| --- | --- |
| [`ItemStack`](ItemStack.md) | 包装原生 endstone::ItemStack 的托管对象。使用 ItemStack.Create("minecraft:diamond", 数量) 创建,可读写附魔、显示名与 lore。 |
| [`Inventory`](Inventory.md) | 包装原生 endstone::Inventory 的托管对象,提供物品的读取、添加、移除与容量查询。 |
| [`PlayerInventory`](PlayerInventory.md) | 玩家的背包(继承 Inventory),提供主手/副手与护甲槽的快速访问。 |

## 界面

| 类型 | 说明 |
| --- | --- |
| [`FormBase`1`](FormBase.md) | 三种表单(MessageForm/ActionForm/ModalForm)的流式基类。Send() 把所有权交给原生侧,提交/关闭时自动注销回调;发送后不可复用构建器。 |
| [`MessageForm`](MessageForm.md) | 带标题、内容与两个按钮的简单表单。 |
| [`ActionForm`](ActionForm.md) | 动作表单:标题 + 内容 + 任意数量的带图标按钮。 |
| [`ModalForm`](ModalForm.md) | 模态表单:标题 + 内容 + 多个控件(下拉、滑动条、文本输入、开关等),提交结果为 JSON 负载。 |
| [`BossBar`](BossBar.md) | 包装原生 endstone::BossBar 的托管对象。Boss 血条可以添加进度的文字说明与颜色/分段样式,并绑定到多个玩家。 |
| [`MapView`](MapView.md) | 包装原生 endstone::MapView 的托管对象。服务器通过 CreateMap 创建地图视图,可调整缩放、中心与跟踪设置,并添加渲染器。 |
| [`MapCanvas`](MapCanvas.md) | 地图画布:通过 SetPixel/SetPixelColor 逐像素绘制地图渲染内容。 |
| [`MapCursor`](MapCursor.md) | 地图上的游标对象(坐标、方向、类型、可见性)。 |
| [`MapColor`](MapColor.md) | 地图画布像素使用的 RGBA 颜色值类型。 |
| [`MapRenderer`](MapRenderer.md) | 地图渲染器基类:重写 Render() 在 MapCanvas 上绘制,通过 MapView.AddRenderer 添加。 |

## 原生互操作

| 类型 | 说明 |
| --- | --- |
| [`Bootstrap`](Bootstrap.md) | 原生 C++ 加载器 (dotnet_loader) 调用的托管入口点。包含宿主初始化、插件装配、事件/表单/调度/地图渲染等回调转发逻辑。 |

## 事件与枚举

| 页面 | 内容 |
| --- | --- |
| [事件](events.md) | 派生自 `Event` 的 55 个事件类 |
| [枚举](enums.md) | 16 个公开枚举 |

