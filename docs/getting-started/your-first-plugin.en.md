# Your First Plugin

!!! tip "Recommended: use the EndstoneDotnet.Toolchain"

    Managing the project, reference paths and deployment by hand is tedious. We recommend
    installing [EndstoneDotnet.Toolchain](https://github.com/ArkMirage/EndstoneDotnet.Toolchain),
    a companion application that creates, builds and deploys .NET plugins for Endstone in a few
    clicks. See [Using EndstoneDotnet.Toolchain](toolchain.md) for a guided walkthrough.

## 1. Create the project

Create a class library targeting `net10.0` and reference `Endstone.Loader.dll`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- The assembly name must end with .Plugin; the loader matches *.Plugin.dll -->
    <AssemblyName>My.Plugin</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Adjust to the actual path of Endstone.Loader.dll -->
    <Reference Include="Endstone.Loader">
      <HintPath>..\Endstone.Loader.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

You can also use a `<ProjectReference>` (as the example project does); `Endstone.Loader.dll`
is then not copied next to your plugin.

## 2. The plugin main class

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

Notes:

- The class must derive from `PluginBase` and carry the `[Plugin]` attribute
- The plugin name (`my_plugin`) may only contain lowercase letters, digits and underscores
  (Endstone requirement)
- `OnLoad` runs when the plugin is loaded (the world may not be ready yet), `OnEnable` runs when
  it is enabled (world available), `OnDisable` runs when the server shuts down / the plugin is unloaded
- The `Logger` property writes to the server's plugin log: `Trace` is filtered out by default,
  `Info` and above are visible

## 3. Register commands

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

A `CommandSender` can be a player or the console: check `sender.IsPlayer`, cast with
`sender.AsPlayer()`. Players without the required permission are rejected automatically by Endstone.

## 4. Register events

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

`RegisterEvent<T>` registers by event type; `T` must be an event class in the
`Endstone.Loader` namespace. `EventPriority` controls handler ordering across plugins.
Setting `e.IsCancelled = true` cancels the event.

## 5. Scheduler (delayed / periodic tasks)

```csharp
// Run once after 40 ticks (2 seconds), synchronous (server main thread)
var task = Scheduler.RunTaskLater(() => Logger.Info("Sync task fired"), 40);

// Periodic task: every 20 ticks, first run after 10 ticks
var timer = Scheduler.RunTaskTimer(() => Logger.Info("Once per second"), 10, 20);

// Async task: runs on a detached thread
var asyncTask = Scheduler.RunTaskLaterAsync(() =>
{
    // Pure managed-side computation only; BDS APIs are FORBIDDEN here
}, 40);

// Cancellation
timer.Cancel();
Scheduler.CancelAll();                     // automatic on plugin disable
var pending = Scheduler.GetPendingTasks(); // list all queued tasks
```

!!! danger "Thread safety"

    Async tasks (`RunTaskAsync`, `RunTaskLaterAsync`, `RunTaskTimerAsync`) run on the native
    scheduler's worker thread. Calling any BDS-side API (SendMessage, BroadcastMessage,
    world/entity manipulation, ...) from those callbacks is **strictly forbidden and will crash
    the server**. Async callbacks should do pure managed computation only; to touch the game
    world, marshal back to the main thread with a sync task first.

## 6. Other capabilities

- Forms: `player.ShowForm(new MessageForm()... / ActionForm / ModalForm)`
- Boss bars: `Server.CreateBossBar(title, color, style, flags)`
- Maps: `Server.CreateMap(dimension)`, `MapView.AddRenderer(MapRenderer)`, `player.SendMap(map)`
- Inventory: `player.Inventory` (read / add / remove / armor slots)
- Items: `ItemStack.Create("minecraft:diamond", 1)`
- Enchantments: `Enchantment.Get("minecraft:sharpness")`, `item.AddEnchant(id, level, force: false)`
- Blocks: `dimension.GetBlockAt(x, y, z)`, `block.CaptureState()`
- Actors: `dimension.SpawnActor(type, location)`, `player.SpawnMob(type)`
- Level: `Server.Level`, `level.GetDimension("overworld")`, `dimension.GetLoadedChunks()`
- Messages: `Server.BroadcastMessage(...)`, `player.SendTitle(...)`, `player.SendToast(...)`

See the [API Reference](../reference/csharp/index.md) for the full surface.

## 7. Build and deploy

```
dotnet build -c Release
```

Copy `*.Plugin.dll` from `bin\Release\net10.0\` into the `plugins.net\` folder in the server
root, then restart the server. During development you can also copy the `.pdb` file for
breakpoint symbols.

!!! tip "Without the toolchain, merge your dependencies with ILRepack"

    The loader treats each `*.Plugin.dll` file in `plugins.net\` as one plugin and the
    deployment guide above copies only the main assembly. If your plugin depends on
    third-party packages, merge all dependency DLLs into the main assembly with
    [ILRepack](https://github.com/gluck/il-repack) so that a single `*.Plugin.dll` is
    self-contained. Dependencies that must load in their own context (e.g. native bindings)
    can be kept separate.