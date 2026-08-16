# Endstone DotNet Loader

在 [Endstone](https://endstone.dev/) BDS 服务器上运行 .NET (C#) 插件的加载器。
本文档面向服务器管理员与插件开发者。

!!! warning

    仅支持 **.NET 10** — 其他目标框架版本的插件将无法运行。

## 特性

- 在 Endstone 基岩版服务器上运行 C# 插件程序集 (`*.Plugin.dll`)
- 完整的 endstone API:命令、事件、调度器、表单、boss 血条、地图、背包、附魔、服务等
- 插件级 `AssemblyLoadContext`:每个插件可携带自己的依赖
- 支持 **Windows x64** 与 **Linux x64**

## 快速入口

- [安装](getting-started/installation.md) - 在服务器上安装加载器
- [第一个插件](getting-started/your-first-plugin.md) - 编写并部署 C# 插件
- [API 参考](reference/csharp/index.md) - 自动生成、中英双文的 API 文档

## 示例

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