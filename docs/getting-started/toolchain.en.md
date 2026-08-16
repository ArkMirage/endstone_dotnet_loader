# Using EndstoneDotnet.Toolchain

[EndstoneDotnet.Toolchain](https://github.com/ArkMirage/EndstoneDotnet.Toolchain) is the
recommended companion tool for Endstone .NET plugins. It automates the two most annoying
parts of plugin development:

1. **Merging dependencies** — on every build, all dependency DLLs of your plugin are merged
   into the main assembly via **ILRepack**, so you ship a single clean assembly;
2. **Packaging** — the merged assembly (plus any DLLs you chose to keep separate) is assembled
   into an `endstone-plugin` folder, ready to drop into the server's `plugins` directory.

A build then produces exactly one deployable folder — no manual copying of dependency DLLs.

## Requirements

- .NET 8 runtime (for the tool itself) and the .NET SDK (to build plugins)
- Windows recommended (interactive console TUI); the merge logic itself is cross-platform

## 1. Get the tool

Clone or download the repository:

```
git clone https://github.com/ArkMirage/EndstoneDotnet.Toolchain
```

Build it:

```
cd EndstoneDotnet.Toolchain
dotnet publish -c Release -r win-x64 --self-contained false
```

Put `EndstoneDotnet.Toolchain.exe` into your plugin project root directory (or any
subdirectory) — the tool walks up to find your `.csproj`.

## 2. Configure once

Run the tool (double-click it, or invoke with `configure`):

```
EndstoneDotnet.Toolchain.exe
```

It will:

1. Find the directory containing your `.csproj`;
2. Auto-write `<Import Project="EndstoneDotnet.Toolchain.targets" />` into the project file
   if it is missing;
3. Open the interactive TUI listing your plugin's dependency DLLs — tick the ones you want to
   **keep separate** (not merged; e.g. native binding assemblies that must load on their own),
   then press `Q` / `Esc` to save.

The selections are stored in `endstone-toolchain.json` next to your project:

```json
{
  "excludedAssemblies": [
    "MyNativeBridge"
  ]
}
```

TUI keys: `↑`/`↓` move, `Enter`/`Space` toggle, `A` selects all, `N` clears, `Q`/`Esc` saves
and exits. When stdin is redirected (CI), the TUI degrades to "keep existing config" mode.

## 3. Build normally

```
dotnet build
```

Nothing changes in your workflow — the `AfterBuild` hook runs ILRepack automatically. The
output ends up at:

```
bin\<Configuration>\<TargetFramework>\endstone-plugin\
```

## 4. Deploy

Copy the whole `endstone-plugin` folder into the server's `plugins` directory and restart
the server. One folder, one drop — done.

## Command-line reference

| Command | Description |
| --- | --- |
| `EndstoneDotnet.Toolchain.exe` | Open the TUI, pick assemblies to keep separate, save config |
| `configure` / `-c` / `--configure` | Same as above |
| `merge --project <dir> --output <dir> [--assembly <file>]` | Run the ILRepack merge (invoked by the build hook) |
| `help` / `-h` / `--help` | Show help |

`merge` options:

| Option | Required | Description |
| --- | --- | --- |
| `--project` | Yes | Plugin project root (locates `endstone-toolchain.json`) |
| `--output` | Yes | Build output directory (`TargetDir`) |
| `--assembly` | No | Main assembly file name (default: `<project dir name>.dll`) |

MSBuild properties (set in `.targets`):

| Property | Description |
| --- | --- |
| `EndstoneDotnetToolchainDisabled` | Set to `true` to skip merging for this build |
| `EndstoneDotnetToolchainExe` | Override tool exe path (default: same directory as the targets file) |

## Troubleshooting

- **Plugin fails to load with missing types** — an assembly that must load in its own context
  was merged: re-run `configure` and mark it "keep separate".
- **Merge is skipped entirely** — check that `EndstoneDotnetToolchainDisabled` is not set and
  that the `<Import Project="EndstoneDotnet.Toolchain.targets" />` line is present in your
  `.csproj` after a fresh clone.
- **Corrupt config** — the tool backs the file up as `endstone-toolchain.json.corrupt.bak`
  automatically; just run `configure` again.