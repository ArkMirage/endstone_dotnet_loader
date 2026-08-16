# EndstoneDotnet.Toolchain 使用教程

[EndstoneDotnet.Toolchain](https://github.com/ArkMirage/EndstoneDotnet.Toolchain)
是 Endstone .NET 插件的推荐配套工具。它自动完成插件开发中最繁琐的两件事:

1. **合并依赖** —— 每次构建时,通过 **ILRepack** 把插件的所有依赖 DLL 合并进主程序集,
   产出单个干净的程序集;
2. **打包输出** —— 把合并后的程序集(以及你选择保留的独立 DLL)组装成
   `endstone-plugin` 文件夹,可直接丢进服务器的 `plugins` 目录。

构建一次,得到一个可直接部署的文件夹,无需手动拷贝依赖 DLL。

## 环境要求

- .NET 8 运行时(工具本身)+ .NET SDK(构建插件)
- 建议 Windows(交互式控制台 TUI);合并逻辑本身跨平台

## 1. 获取工具

克隆或下载仓库:

```
git clone https://github.com/ArkMirage/EndstoneDotnet.Toolchain
```

构建:

```
cd EndstoneDotnet.Toolchain
dotnet publish -c Release -r win-x64 --self-contained false
```

把 `EndstoneDotnet.Toolchain.exe` 放进插件项目的根目录(或任意子目录)——工具会自动
向上查找你的 `.csproj`。

## 2. 配置一次

运行工具(双击,或带 `configure` 参数执行):

```
EndstoneDotnet.Toolchain.exe
```

它会:

1. 找到包含 `.csproj` 的目录;
2. 如果项目文件缺少 `<Import Project="EndstoneDotnet.Toolchain.targets" />` 则自动写入;
3. 打开交互式 TUI 列出插件的依赖 DLL——勾选需要**保持独立**(不合并,例如必须单独加载的
   原生绑定程序集)的那些,按 `Q` / `Esc` 保存。

选择记录在项目旁的 `endstone-toolchain.json` 中:

```json
{
  "excludedAssemblies": [
    "MyNativeBridge"
  ]
}
```

TUI 按键:`↑`/`↓` 移动,`Enter`/`空格` 切换,`A` 全选,`N` 清空,`Q`/`Esc` 保存并退出。
stdin 被重定向(CI)时,TUI 自动降级为"保留现有配置"模式。

## 3. 正常构建

```
dotnet build
```

工作流程没有任何变化——`AfterBuild` 钩子会自动执行 ILRepack。输出位于:

```
bin\<Configuration>\<TargetFramework>\endstone-plugin\
```

## 4. 部署

把整个 `endstone-plugin` 文件夹复制到服务器的 `plugins` 目录,重启服务器即可。
一个文件夹、一次拷贝,完成。

## 命令行参考

| 命令 | 说明 |
| --- | --- |
| `EndstoneDotnet.Toolchain.exe` | 打开 TUI,选择保持独立的程序集,保存配置 |
| `configure` / `-c` / `--configure` | 同上 |
| `merge --project <目录> --output <目录> [--assembly <文件>]` | 执行 ILRepack 合并(构建钩子调用) |
| `help` / `-h` / `--help` | 显示帮助 |

`merge` 选项:

| 选项 | 必填 | 说明 |
| --- | --- | --- |
| `--project` | 是 | 插件项目根目录(用于定位 `endstone-toolchain.json`) |
| `--output` | 是 | 构建输出目录(`TargetDir`) |
| `--assembly` | 否 | 主程序集文件名(默认:`<项目目录名>.dll`) |

MSBuild 属性(在 `.targets` 中设置):

| 属性 | 说明 |
| --- | --- |
| `EndstoneDotnetToolchainDisabled` | 设为 `true` 跳过本次合并 |
| `EndstoneDotnetToolchainExe` | 覆盖工具 exe 路径(默认:targets 文件同目录) |

## 常见问题

- **插件加载报缺少类型** —— 某个必须独立加载的程序集被合并了:重新运行 `configure`
  把它标记为"保持独立"。
- **合并根本没执行** —— 检查 `EndstoneDotnetToolchainDisabled` 未设置,且 `.csproj`
  中存在 `<Import Project="EndstoneDotnet.Toolchain.targets" />`(重新克隆后需要重新配置)。
- **配置文件损坏** —— 工具会自动备份为 `endstone-toolchain.json.corrupt.bak`,
  重新运行 `configure` 即可。