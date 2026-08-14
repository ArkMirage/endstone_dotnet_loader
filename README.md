# endstone-dotnet-loader Tutorial

A loader that runs .NET (C#) plugins on an Endstone BDS server. This document is intended for both server administrators and plugin developers.

Version: v0.1.0 (built against Endstone 0.11.8 / Minecraft 26.40)

---

## 1. Requirements

- Windows x64
- An Endstone BDS server (Windows), Endstone 0.11.x recommended
- .NET 10 Runtime, satisfied by either:
  - .NET 10 installed on the system (run `dotnet --list-runtimes` and look for `Microsoft.NETCore.App 10.x`), or
  - A copy of a .NET 10 installation folder, with the following environment variable set before the server starts:

    ```
    set ENDSTONE_DOTNET_PATH=D:\server\dotnet10
    ```

  The loader looks for a .NET runtime in this order: the `ENDSTONE_DOTNET_PATH` environment variable, then the system-wide installation. If neither exists, the loader is not started and .NET plugins are skipped (the server still boots normally and an error is printed).
- VC++ 14.x redistributable (preinstalled on virtually all Windows systems; if missing, the plugin fails to load with an error such as "msvcp140.dll not found").

---

## 2. Installation

1. Extract the release zip and copy the `plugins` and `plugins.net` folders into the server root directory (next to `bedrock_server.exe`). The final layout is:

   ```
   <server root>\
     bedrock_server.exe
     plugins\
       endstone_dotnet_loader.dll
       dotnet_loader\
         endstone_dotnet_loader.dll
         runtime\
           Endstone.Loader.dll
           Endstone.Loader.runtimeconfig.json
           Endstone.Loader.deps.json
     plugins.net\
       Example.Plugin.dll
   ```

   Note: `endstone_dotnet_loader.dll` exists in two places under `plugins\` (`plugins\` and `plugins\dotnet_loader\`). Both are required.

2. (Optional but recommended) Clear the `plugins\.local\` folder. It is the plugin loading cache; stale cache entries can cause problems after an upgrade.

3. Start the server. A successful startup looks like this:

   ```
   [DotNetLoader] Loading dotnet_loader v0.1.0
   [DotNetLoader] .NET runtime started.
   [ExamplePlugin] Loading example_plugin v1.0.0
   [DotNetLoader] Loaded 1 .NET plugin(s) from 'D:\endstone_bds\bedrock_server\plugins.net'.
   [ExamplePlugin] Enabling example_plugin v1.0.0
   ```

4. Join the game and run `/example hello` (alias `/ex hello`). A reply "Hello, <your name>!" means everything works.

---

## 3. Verifying the example plugin

The bundled example plugin `Example.Plugin.dll` provides the following commands (from in-game or the console):

| Command | Purpose |
| --- | --- |
| `/ex test` | Sends `.test` in chat to trigger the 30-item automated API self-test |
| `/ex hello` | Greeting message |
| `/ex whoami` | Shows info about you / the console |
| `/ex item` | Inspects the full attributes of the item in hand |
| `/ex tag <show\|hide\|always\|score\|sb>` | Name tag and scoreboard tag operations |
| `/ex mob <type> [name] [health]` | Spawns a mob |
| `/ex form <message\|action\|modal>` | The three form types (player only) |
| `/ex boss <show\|hide>` | Boss bar (player only) |
| `/ex level [time\|block\|highest\|spawn\|drop\|chunks]` | Level / dimension operations |
| `/ex map <create\|send\|item\|clear>` | Map and map renderer |
| `/ex inv <show\|give\|slot\|clear>` | Inventory operations |
| `/ex sched <once\|async\|timer\|stop\|pending>` | Scheduler demo |

Typing `dotnet-test` in the console triggers a broadcast to verify the console event pipeline.

---

## 4. Writing your own .NET plugin

### 4.1 Creating the project

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

Tip: like the example project you can also use a `<ProjectReference>`; Endstone.Loader.dll is then not copied next to your plugin.

### 4.2 The plugin main class

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
- The plugin name (`my_plugin`) may only contain lowercase letters, digits and underscores (Endstone requirement)
- `OnLoad` runs when the plugin is loaded (the world may not be ready yet), `OnEnable` runs when it is enabled (world available), `OnDisable` runs when the server shuts down / the plugin is unloaded
- The `Logger` property writes to the server's plugin log. Console levels: `Trace` is filtered out by default; `Info` and above are visible

### 4.3 Registering commands

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

Notes:

- `Command(name)` returns a fluent builder; `Handler` receives `(CommandSender, IReadOnlyList<string>)` and returns `true` when handled
- A `CommandSender` can be a player or the console: check `sender.IsPlayer`, cast with `sender.AsPlayer()`
- Players without the required permission are rejected automatically by Endstone

### 4.4 Registering events

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

Notes:

- `RegisterEvent<T>` registers by event type; `T` must be an event class in the `Endstone.Loader` namespace
- `EventPriority` controls handler ordering across plugins: `Lowest / Low / Normal / High / Highest / Monitor`
- Setting `e.IsCancelled = true` cancels the event (e.g. intercepting chat)
- Common events: `PlayerJoinEvent`, `PlayerQuitEvent`, `PlayerChatEvent`, `PlayerCommandEvent`, `PlayerDeathEvent`, `PlayerInteractEvent`, `ServerCommandEvent`, and more

### 4.5 Scheduler (delayed / periodic tasks)

```csharp
// Run once after 40 ticks (2 seconds), synchronous (server main thread)
var task = Scheduler.RunTaskLater(() =>
{
    Logger.Info("Sync task fired");
}, 40);

// Periodic task: every 20 ticks, first run after 10 ticks
var timer = Scheduler.RunTaskTimer(() =>
{
    Logger.Info("Once per second");
}, 10, 20);

// Async task: runs on a detached thread; thread-safety rules below apply
var asyncTask = Scheduler.RunTaskLaterAsync(() =>
{
    // Pure managed-side computation only; BDS APIs are FORBIDDEN here
}, 40);

// Cancellation
timer.Cancel();
Scheduler.CancelAll();                    // automatic on plugin disable, no need to call manually
var pending = Scheduler.GetPendingTasks(); // list all queued tasks
```

Thread-safety warning: async tasks (`RunTaskAsync`, `RunTaskLaterAsync`, `RunTaskTimerAsync`) run on the native scheduler's worker thread. Calling any BDS-side API (SendMessage, BroadcastMessage, world/entity manipulation, ...) from those callbacks is strictly forbidden and will crash the server. Async callbacks should do pure managed computation only; to touch the game world, marshal back to the main thread with a sync task first.

### 4.6 Other capabilities

The example plugin demonstrates the full feature list:

- Forms: `player.ShowForm(new MessageForm()... / ActionForm / ModalForm)`
- Boss bars: `Server.CreateBossBar(title, color, style, flags)`
- Maps: `Server.CreateMap(dimension)`, `MapView.AddRenderer(MapRenderer)`, `player.SendMap(map)`
- Inventory: `player.Inventory` (read / add / remove / armor slots)
- Items: `ItemStack.Create("minecraft:diamond", 1)`
- Blocks and block states: `dimension.GetBlockAt(x, y, z)`, `block.CaptureState()`
- Actors: `dimension.SpawnActor(type, location)`, `player.SpawnMob(type)`
- Level: `Server.Level`, `level.GetDimension("overworld")`, `dimension.GetLoadedChunks()`
- Broadcast and messages: `Server.BroadcastMessage(...)`, `player.SendTitle(...)`, `player.SendToast(...)`, `player.SendTip(...)`
- Permissions: `Command(...).Permission("some.permission")`

### 4.7 Building and deploying

```
dotnet build -c Release
```

Copy `*.Plugin.dll` from `bin\Release\net10.0\` (the file whose assembly name ends with `.Plugin`) into the `plugins.net\` folder in the server root, then restart the server (or run `stop` and start again). During development you can also copy the `.pdb` file for breakpoint symbols.

---

## 5. Updating and uninstalling

Update: stop the server, overwrite the matching files in `plugins\` and `plugins.net\`, clear `plugins\.local\`, restart.

Uninstall: stop the server, delete `endstone_dotnet_loader.dll` from `plugins\`, delete the whole `plugins\dotnet_loader\` folder and the corresponding `*.Plugin.dll` files from `plugins.net\`, restart.

---

## 6. Troubleshooting

**Q1: Log shows "Failed to start the .NET runtime" / "Unable to locate hostfxr.dll"**
The .NET 10 runtime was not found. Install the .NET 10 Runtime, or point `ENDSTONE_DOTNET_PATH` at a directory containing .NET 10 and start the server again.

**Q2: "Failed to start the .NET runtime" complaining about missing files in the runtime dir**
`Endstone.Loader.dll` or `Endstone.Loader.runtimeconfig.json` is missing under `plugins\dotnet_loader\runtime\`. Re-extract the full release package.

**Q3: A plugin fails to load ("cannot load plugins.net\xxx.Plugin.dll")**
Check that the file name ends with `.Plugin.dll`, the plugin targets net10.0, and a matching .NET 10 runtime exists on the server.

**Q4: The plugin still behaves like an old version after an upgrade**
Clear the `plugins\.local\` cache and restart the server.

**Q5: "msvcp140.dll / vcruntime140.dll not found" in the console**
Install the Microsoft Visual C++ 14.x Redistributable.

**Q6: Some `.test` self-test items fail**
Some checks depend on player state (e.g. the "item in hand" item requires holding an item). For anything else, report the `[AutoTest] FAIL` lines from the log.

**Q7: The server crashes when an async scheduled task calls game APIs**
Async callbacks run off the main thread and BDS APIs are not thread-safe. Keep async callbacks pure-managed, or use sync tasks instead.

---

## 7. Known limitations

- Windows x64 only
- The loader is compiled against a specific Endstone SDK snapshot fetched from GitHub at build time; if the server's Endstone moves to an incompatible major version, the loader must be rebuilt
- Plugins run on .NET 10 (the runtimeconfig is pinned)
