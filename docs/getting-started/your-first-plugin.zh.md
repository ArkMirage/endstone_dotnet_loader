# 你的第一个插件

!!! tip "推荐:配合 EndstoneDotnet.Toolchain 使用"

    手工管理项目、引用路径与部署比较繁琐。推荐安装
    [EndstoneDotnet.Toolchain](https://github.com/ArkMirage/EndstoneDotnet.Toolchain)
    配套应用,几步点击即可创建、构建并部署 .NET 插件。教程见
    [EndstoneDotnet.Toolchain 使用教程](toolchain.md)。

## 1. 创建项目

创建一个目标框架为 `net10.0` 的类库,并引用 `Endstone.Loader.dll`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- 程序集名称必须以 .Plugin 结尾;加载器匹配 *.Plugin.dll -->
    <AssemblyName>My.Plugin</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- 按 Endstone.Loader.dll 的实际路径调整 -->
    <Reference Include="Endstone.Loader">
      <HintPath>..\Endstone.Loader.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

也可以像示例项目一样使用 `<ProjectReference>`;这样 `Endstone.Loader.dll` 不会复制到插件旁边。

## 2. 插件主类

```csharp
using Endstone.Loader;

namespace My.Plugin;

[Plugin("my_plugin", "1.0.0",
    Description = "My first .NET plugin",
    Authors = ["Someone"])]
public sealed class MyPlugin : PluginBase
{
    public override void OnLoad()
    {
        Logger.Info("Plugin loading");
    }

    public override void OnEnable()
    {
        Logger.Info("Plugin enabled");
    }

    public override void OnDisable()
    {
        Logger.Info("Plugin disabled");
    }
}
```

说明:

- 类必须继承 `PluginBase` 并带有 `[Plugin]` 特性
- 插件名称(`my_plugin`)只能包含小写字母、数字与下划线(Endstone 要求)
- `OnLoad` 在插件加载时调用(世界可能尚未就绪),`OnEnable` 在启用时调用(世界可用),
  `OnDisable` 在服务器关闭 / 插件卸载时调用
- `Logger` 属性写入服务器的插件日志:`Trace` 默认被过滤,`Info` 及以上可见

## 3. 注册命令

```csharp
public override void OnLoad()
{
    Command("hello")
        .Description("Say hello")
        .Usage("/hello", "/hello <name>")
        .Alias("hi")
        .Permission("myplugin.command.hello")
        .Handler(OnHelloCommand);
}

private bool OnHelloCommand(CommandSender sender, IReadOnlyList<string> args)
{
    var name = args.Count > 0 ? args[0] : sender.Name;
    sender.SendMessage("Hello, {0}!", name);
    return true;
}
```

`CommandSender` 既可以是玩家也可以是控制台:用 `sender.IsPlayer` 判断,用 `sender.AsPlayer()` 转型。
缺少所需权限的玩家会被 Endstone 自动拒绝。

## 4. 注册事件

```csharp
public override void OnEnable()
{
    RegisterEvent<PlayerJoinEvent>(OnPlayerJoin);
    RegisterEvent<PlayerChatEvent>(OnPlayerChat, EventPriority.High);
}

private void OnPlayerJoin(PlayerJoinEvent e)
{
    var player = e.Player!;
    Server.BroadcastMessage($"Welcome {player.Name}!");
}

private void OnPlayerChat(PlayerChatEvent e)
{
    if (e.Message == "ping")
    {
        e.IsCancelled = true;
        e.Player!.SendMessage("pong!");
    }
}
```

`RegisterEvent<T>` 按事件类型注册;`T` 必须是 `Endstone.Loader` 命名空间内的事件类。
`EventPriority` 控制跨插件的处理顺序。设置 `e.IsCancelled = true` 可取消事件。

## 5. 调度器(延迟 / 周期任务)

```csharp
// 40 tick(2 秒)后执行一次,同步(服务器主线程)
var task = Scheduler.RunTaskLater(() => Logger.Info("Sync task fired"), 40);

// 周期任务:每 20 tick 执行,首次在 10 tick 后
var timer = Scheduler.RunTaskTimer(() => Logger.Info("Once per second"), 10, 20);

// 异步任务:在独立线程上运行
var asyncTask = Scheduler.RunTaskLaterAsync(() =>
{
    // 只做纯托管计算;BDS API 在这里被严格禁止
}, 40);

// 取消
timer.Cancel();
Scheduler.CancelAll();                     // 插件停用时会自动执行
var pending = Scheduler.GetPendingTasks(); // 列出所有排队任务
```

!!! danger "线程安全"

    异步任务(`RunTaskAsync`、`RunTaskLaterAsync`、`RunTaskTimerAsync`)运行在原生调度器的
    工作线程上。在这些回调中调用任何 BDS 侧 API(SendMessage、BroadcastMessage、世界/实体操作等)
    是**严格禁止的,会导致服务器崩溃**。异步回调只应做纯托管计算;需要操作游戏世界时,
    先用同步任务回到主线程。

## 6. 其他能力

- 表单:`player.ShowForm(new MessageForm()... / ActionForm / ModalForm)`
- boss 血条:`Server.CreateBossBar(title, color, style, flags)`
- 地图:`Server.CreateMap(dimension)`、`MapView.AddRenderer(MapRenderer)`、`player.SendMap(map)`
- 背包:`player.Inventory`(读 / 加 / 删 / 护甲槽)
- 物品:`ItemStack.Create("minecraft:diamond", 1)`
- 附魔:`Enchantment.Get("minecraft:sharpness")`、`item.AddEnchant(id, level, force: false)`
- 方块:`dimension.GetBlockAt(x, y, z)`、`block.CaptureState()`
- 实体:`dimension.SpawnActor(type, location)`、`player.SpawnMob(type)`
- 世界:`Server.Level`、`level.GetDimension("overworld")`、`dimension.GetLoadedChunks()`
- 消息:`Server.BroadcastMessage(...)`、`player.SendTitle(...)`、`player.SendToast(...)`

完整 API 见[API 参考](../reference/csharp/index.md)。

## 7. 构建与部署

```
dotnet build -c Release
```

把 `bin\Release\net10.0\` 中的 `*.Plugin.dll`(程序集名以 `.Plugin` 结尾的那个文件)复制到
服务器根目录的 `plugins.net\` 文件夹,然后重启服务器。开发时也可以一并复制 `.pdb` 文件
以获得断点符号。

!!! tip "不使用工具链的话,用 ILRepack 合并依赖"

    加载器把 `plugins.net\` 中的每个 `*.Plugin.dll` 文件当作一个插件,而上面的部署步骤只
    复制主程序集。如果插件依赖第三方包,请用 [ILRepack](https://github.com/gluck/il-repack)
    把所有依赖 DLL 合并进主程序集,使单个 `*.Plugin.dll` 自包含。需要在独立上下文加载的
    依赖(如原生绑定)可保留为独立文件。