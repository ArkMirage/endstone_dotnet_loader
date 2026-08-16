# Installation

## Requirements

- **Windows x64** or **Linux x64**
- An Endstone BDS server for the matching OS (Endstone 0.11.x recommended)
- .NET 10 Runtime, satisfied by either:
    - .NET 10 installed on the system (run `dotnet --list-runtimes` and look for `Microsoft.NETCore.App 10.x`), or
    - A copy of a .NET 10 installation folder, with `ENDSTONE_DOTNET_PATH` set before the server starts:

        ```
        set ENDSTONE_DOTNET_PATH=D:\server\dotnet10          # Windows
        export ENDSTONE_DOTNET_PATH=/opt/dotnet10            # Linux
        ```

    The loader looks for a .NET runtime in this order: the `ENDSTONE_DOTNET_PATH` environment
    variable, then the system-wide installation. If neither exists, the loader is not started and
    .NET plugins are skipped (the server still boots normally and an error is printed).
- Windows: VC++ 14.x redistributable (missing it produces e.g. "msvcp140.dll not found")
- Linux: shared libraries required by the Endstone native runtime (e.g. `libc++`)

## Steps

1. Extract the release zip for your OS and copy the `plugins` and `plugins.net` folders into the
   server root directory (next to `bedrock_server`). The final layout is:

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

    Note: `endstone_dotnet_loader.dll` / `.so` exists in two places under `plugins\`
    (`plugins\` and `plugins\dotnet_loader\`). Both are required.

2. (Optional but recommended) Clear the `plugins\.local\` folder. It is the plugin loading cache;
   stale cache entries can cause problems after an upgrade.

3. Start the server. A successful startup looks like this:

    ```
    [DotNetLoader] Loading dotnet_loader
    [DotNetLoader] .NET runtime started.
    [ExamplePlugin] Loading example_plugin v1.0.0
    [DotNetLoader] Loaded 1 .NET plugin(s) from '.../plugins.net'.
    [ExamplePlugin] Enabling example_plugin v1.0.0
    ```

4. Join the game and run `/example hello` (alias `/ex hello`). A reply "Hello, <your name>!"
   means everything works.

## Verifying the example plugin

The bundled example plugin `Example.Plugin.dll` also provides these commands:

| Command | Purpose |
| --- | --- |
| `/ex test` | Sends `.test` in chat to trigger the 30-item automated API self-test |
| `/ex whoami` | Shows info about you / the console |
| `/ex item` | Inspects the full attributes of the item in hand |
| `/ex enchant <list\|info\|add\|remove\|clear> [id] [level] [force]` | Enchantments on the held item |
| `/ex tag <show\|hide\|always\|score\|sb>` | Name tag and scoreboard tag operations |
| `/ex mob <type> [name] [health]` | Spawns a mob |
| `/ex form <message\|action\|modal>` | The three form types (player only) |
| `/ex boss <show\|hide>` | Boss bar (player only) |
| `/ex level [time\|block\|highest\|spawn\|drop\|chunks]` | Level / dimension operations |
| `/ex map <create\|send\|item\|clear>` | Map and map renderer |
| `/ex inv <show\|give\|slot\|clear>` | Inventory operations |
| `/ex sched <once\|async\|timer\|stop\|pending>` | Scheduler demo |

Typing `dotnet-test` in the console triggers a broadcast to verify the console event pipeline.

## Updating and uninstalling

- **Update**: stop the server, overwrite the matching files in `plugins\` and `plugins.net\`,
  clear `plugins\.local\`, restart.
- **Uninstall**: stop the server, delete `endstone_dotnet_loader.dll` (Windows) /
  `endstone_dotnet_loader.so` (Linux) from `plugins\`, delete the whole `plugins\dotnet_loader\`
  folder and the corresponding `*.Plugin.dll` files from `plugins.net\`, restart.