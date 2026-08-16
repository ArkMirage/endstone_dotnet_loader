# endstone-dotnet-loader 教程

> **English**: [English](./README.md)

一个在 Endstone BDS 服务器上运行 .NET (C#) 插件的加载器。本文档同时面向服务器管理员与插件开发者，覆盖 **Windows x64** 与 **Linux x64**。

> **文档站点**: [https://arkmirage.github.io/endstone_dotnet_loader/](https://arkmirage.github.io/endstone_dotnet_loader/)

> **注意**：**仅支持 .NET 10** — 面向其他框架版本的插件将无法运行。

---

## 1. 运行需求

- **Windows x64** 或 **Linux x64**
- 对应操作系统的 Endstone BDS 服务器（推荐 Endstone 0.11.x）
- .NET 10 运行时，满足以下任一条件：
  - 系统已安装 .NET 10（运行 `dotnet --list-runtimes`，查找 `Microsoft.NETCore.App 10.x`），或
  - 一份 .NET 10 安装目录的副本，并在服务器启动前设置以下环境变量：

    ```
    set ENDSTONE_DOTNET_PATH=D:\server\dotnet10          # Windows
    export ENDSTONE_DOTNET_PATH=/opt/dotnet10            # Linux
    ```

  加载器按以下顺序查找 .NET 运行时：先 `ENDSTONE_DOTNET_PATH` 环境变量，再系统级安装。两者都不存在时加载器不启动、.NET 插件被跳过（服务器仍正常启动并打印错误）。
- Windows：VC++ 14.x 运行库（几乎所有 Windows 都已预装；若缺失，插件加载失败并报 "msvcp140.dll not found" 之类错误）。
- Linux：Endstone 原生运行时所需的共享库（例如基于 libc++ 构建的服务器需要 `libc++`）；缺失时加载失败并提示具体的 `lib*.so`。

---

## 2. 安装

1. 解压对应操作系统的发行 zip，将 `plugins` 与 `plugins.net` 两个文件夹复制到服务器根目录（与 `bedrock_server` 同级）。最终布局：

   ```
   <server root>\
     bedrock_server
     plugins\
       endstone_dotnet_loader.dll        # Windows
       endstone_dotnet_loader.so         # Linux
       dotnet_loader\
         runtime\
           Endstone.Loader.dll
           Endstone.Loader.runtimeconfig.json
           Endstone.Loader.deps.json
     plugins.net\
       Example.Plugin.dll
   ```

   注意：`endstone_dotnet_loader.dll` / `.so` 在 `plugins\` 下出现两处（`plugins\` 与 `plugins\dotnet_loader\`），两处都是必需的。

2. （可选但推荐）清空 `plugins\.local\` 文件夹。它是插件加载缓存，升级后残留的缓存项可能引发问题。

3. 启动服务器。成功启动的输出类似：

   ```
   [DotNetLoader] Loading dotnet_loader
   [DotNetLoader] .NET runtime started.
   [ExamplePlugin] Loading example_plugin v1.0.0
   [DotNetLoader] Loaded 1 .NET plugin(s) from '.../plugins.net'.
   [ExamplePlugin] Enabling example_plugin v1.0.0
   ```

4. 进入游戏运行 `/example hello`（别名 `/ex hello`）。回复 "Hello, <你的名字>!" 即表示一切正常。

---

## 3. 验证示例插件

内置的示例插件 `Example.Plugin.dll` 提供以下命令（游戏内或控制台均可）：

| 命令 | 用途 |
| --- | --- |
| `/ex test` | 在聊天框发送 `.test` 触发 30 项自动化 API 自检 |
| `/ex hello` | 问候消息 |
| `/ex whoami` | 显示你/控制台的信息 |
| `/ex item` | 检查手持物品的全部属性 |
| `/ex enchant <list\|info\|add\|remove\|clear> [id] [level] [force]` | 手持物品的附魔操作：列出、查看、添加（除非 `force` 否则限制等级）、移除、全部清除 |
| `/ex tag <show\|hide\|always\|score\|sb>` | 名称标签与计分板标签操作 |
| `/ex mob <type> [name] [health]` | 生成一个生物 |
| `/ex form <message\|action\|modal>` | 三种表单类型（仅玩家） |
| `/ex boss <show\|hide>` | BOSS 血条（仅玩家） |
| `/ex level [time\|block\|highest\|spawn\|drop\|chunks]` | 世界/维度操作 |
| `/ex map <create\|send\|item\|clear>` | 地图与地图渲染器 |
| `/ex inv <show\|give\|slot\|clear>` | 背包操作 |
| `/ex sched <once\|async\|timer\|stop\|pending>` | 调度器演示 |

在控制台输入 `dotnet-test` 会触发广播，用于验证控制台事件管道。

---

## 4. 编写你自己的 .NET 插件

### 4.1 创建项目

创建一个面向 `net10.0` 的类库并引用 `Endstone.Loader.dll`：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- 程序集名必须以 .Plugin 结尾；加载器匹配 *.Plugin.dll -->
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

提示：也可以像示例项目那样使用 `<ProjectReference>`，这样 `Endstone.Loader.dll` 不会被复制到插件旁边。

### 4.2 插件主类

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

说明：

- 类必须继承 `PluginBase` 并带有 `[Plugin]` 特性
- 插件名（`my_plugin`）只能包含小写字母、数字和下划线（Endstone 要求）
- `OnLoad` 在插件加载时运行（世界可能尚未就绪）；`OnEnable` 在插件启用时运行（世界已可用）；`OnDisable` 在服务器关闭/插件卸载时运行
- `Logger` 属性写入服务器的插件日志。控制台等级：默认过滤 `Trace`；`Info` 及以上可见

### 4.3 注册命令

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

说明：

- `Command(name)` 返回流式构建器；`Handler` 接收 `(CommandSender, IReadOnlyList<string>)`，处理成功返回 `true`
- `CommandSender` 可能是玩家或控制台：用 `sender.IsPlayer` 判断，用 `sender.AsPlayer()` 转换
- 没有所需权限的玩家会被 Endstone 自动拒绝

### 4.4 注册事件

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

说明：

- `RegisterEvent<T>` 按事件类型注册；`T` 必须是 `Endstone.Loader` 命名空间中的事件类
- `EventPriority` 控制跨插件的处理顺序：`Lowest / Low / Normal / High / Highest / Monitor`
- 设置 `e.IsCancelled = true` 取消事件（例如拦截聊天）
- 常见事件：`PlayerJoinEvent`、`PlayerQuitEvent`、`PlayerChatEvent`、`PlayerCommandEvent`、`PlayerDeathEvent`、`PlayerInteractEvent`、`ServerCommandEvent` 等

### 4.5 调度器（延时 / 周期任务）

```csharp
// 40 tick（2 秒）后执行一次，同步（服务器主线程）
var task = Scheduler.RunTaskLater(() =>
{
    Logger.Info("Sync task fired");
}, 40);

// 周期任务：每 20 tick 一次，10 tick 后首次执行
var timer = Scheduler.RunTaskTimer(() =>
{
    Logger.Info("Once per second");
}, 10, 20);

// 异步任务：在独立线程上运行；必须遵守下面的线程安全规则
var asyncTask = Scheduler.RunTaskLaterAsync(() =>
{
    // 只做纯托管计算；严禁调用 BDS API
}, 40);

// 取消
timer.Cancel();
Scheduler.CancelAll();                    // 插件禁用时自动调用，无需手动
var pending = Scheduler.GetPendingTasks(); // 列出所有排队任务
```

线程安全警告：异步任务（`RunTaskAsync`、`RunTaskLaterAsync`、`RunTaskTimerAsync`）运行在原生调度器的工作线程上。在这些回调中调用任何 BDS 侧 API（SendMessage、BroadcastMessage、世界/实体操作……）是严格禁止的，会导致服务器崩溃。异步回调只能做纯托管计算；要操作游戏世界，请先用同步任务切回主线程。

### 4.6 其他能力

示例插件演示了完整的功能列表：

- 表单：`player.ShowForm(new MessageForm()... / ActionForm / ModalForm)`
- BOSS 血条：`Server.CreateBossBar(title, color, style, flags)`
- 地图：`Server.CreateMap(dimension)`、`MapView.AddRenderer(MapRenderer)`、`player.SendMap(map)`
- 背包：`player.Inventory`（读取 / 添加 / 移除 / 盔甲槽）
- 物品：`ItemStack.Create("minecraft:diamond", 1)`
- 附魔：`Enchantment.Get("minecraft:sharpness")`（或静态常量如 `Enchantment.Sharpness`）、`item.AddEnchant(id, level, force: false)`、`item.RemoveEnchant(id)`、`item.RemoveEnchants()`、`item.GetEnchantLevel(id)`、`item.HasEnchant(id)`、`item.HasConflictingEnchant(id)`、`item.Enchantments`（`(附魔, 等级)` 列表）、`enchant.CanEnchantItem(item)`、`enchant.ConflictsWith(other)`、`enchant.StartLevel` / `enchant.MaxLevel`
- 方块与方块状态：`dimension.GetBlockAt(x, y, z)`、`block.CaptureState()`
- 实体：`dimension.SpawnActor(type, location)`、`player.SpawnMob(type)`
- 世界：`Server.Level`、`level.GetDimension("overworld")`、`dimension.GetLoadedChunks()`
- 广播与消息：`Server.BroadcastMessage(...)`、`player.SendTitle(...)`、`player.SendToast(...)`、`player.SendTip(...)`
- 权限：`Command(...).Permission("some.permission")`

### 4.7 构建与部署

```
dotnet build -c Release
```

把 `bin\Release\net10.0\` 下的 `*.Plugin.dll`（程序集名以 `.Plugin` 结尾的那个）复制到服务器根目录的 `plugins.net\` 文件夹，然后重启服务器（或执行 `stop` 再启动）。开发时也可以把 `.pdb` 一并复制以支持断点符号。

---

## 5. 从源码构建

加载器在构建时通过 CMake `FetchContent` 从 GitHub 拉取对应版本的 Endstone SDK 进行编译，因此首次 configure 需要联网。两个构建脚本输出的目录结构一致，都在 `artifacts\<rid>\` 下：

```
artifacts\<rid>\
  plugins\
    endstone_dotnet_loader.dll / .so
    dotnet_loader\
      runtime\
        Endstone.Loader.dll
        Endstone.Loader.runtimeconfig.json
        Endstone.Loader.deps.json
```

### Windows（`build.bat`）

需要 Visual Studio 的 MSVC 环境：请从 **Developer PowerShell** 启动（或先运行 `VsDevCmd.bat`）。工具链全部取自环境变量，脚本不做任何探测或定义。`PATH` 上需要：`clang-cl`（"适用于 Windows 的 C++ Clang 工具"组件或独立 LLVM）、`ninja`、`cmake`、.NET 10 SDK。

```
build.bat
```

输出到 `artifacts\win-x64\`。

### Linux（`build.sh`）

需要 `cmake`、`ninja`、Clang/LLVM 18+ 及配套 `libc++`、`libc++abi`、.NET 10 SDK。`CC`/`CXX`/`DOTNET_ROOT` 均从环境变量读取（发行版若只提供带版本号的编译器，如 `clang-20`，需要显式设置）：

```
CC=clang-18 CXX=clang++-18 ./build.sh
```

输出到 `artifacts/linux-x64/`。

### GitHub Actions

`.github/workflows/build.yml` 同时构建两个平台，并把打包好的 zip（`endstone_dotnet_loader_<version>_win-x64.zip` / `..._linux-x64.zip`）作为工作流 Artifact 上传。该工作流为**手动触发**：在 Actions 标签页点击运行（workflow_dispatch）。

---

## 6. 更新与卸载

更新：停止服务器，覆盖 `plugins\` 与 `plugins.net\` 中对应的文件，清空 `plugins\.local\`，重启。

卸载：停止服务器，删除 `plugins\` 下的 `endstone_dotnet_loader.dll`（Windows）/ `endstone_dotnet_loader.so`（Linux），删除整个 `plugins\dotnet_loader\` 文件夹及 `plugins.net\` 下对应的 `*.Plugin.dll` 文件，重启。