# Endstone DotNet Loader

A loader that runs .NET (C#) plugins on an [Endstone](https://endstone.dev/) BDS server.
This documentation is intended for both server administrators and plugin developers.

!!! warning

    **.NET 10 only** — plugins targeting other framework versions will not run.

## Features

- Runs C# plugin assemblies (`*.Plugin.dll`) on an Endstone Bedrock server
- Full endstone API surface: commands, events, scheduler, forms, boss bars, maps, inventory, enchantments, services and more
- Plugin-scoped `AssemblyLoadContext`: each plugin carries its own dependencies
- **Windows x64** and **Linux x64** support

## Quick links

- [Installation](getting-started/installation.md) - set up the loader on your server
- [Your first plugin](getting-started/your-first-plugin.md) - write and deploy a C# plugin
- [API Reference](reference/csharp/index.md) - auto-generated, bilingual API documentation

## Example

```csharp
using Endstone.Loader;

[Plugin("hello_world", "1.0.0", Description = "My first .NET plugin")]
public sealed class HelloWorldPlugin : PluginBase
{
    public override void OnEnable()
    {
        Command("hello")
            .Description("Say hello")
            .Handler((sender, args) =>
            {
                sender.SendMessage("Hello, {0}!", sender.Name);
                return true;
            });
    }
}
```