# 安装

## 环境要求

- **Windows x64** 或 **Linux x64**
- 对应操作系统的 Endstone BDS 服务器(推荐 Endstone 0.11.x)
- .NET 10 运行时,满足以下任一条件即可:
    - 系统中已安装 .NET 10(运行 `dotnet --list-runtimes`,查找 `Microsoft.NETCore.App 10.x`),或
    - 一份 .NET 10 安装目录的拷贝,并在服务器启动前设置 `ENDSTONE_DOTNET_PATH`:

        ```
        set ENDSTONE_DOTNET_PATH=D:\server\dotnet10          # Windows
        export ENDSTONE_DOTNET_PATH=/opt/dotnet10            # Linux
        ```

    加载器按以下顺序查找 .NET 运行时:`ENDSTONE_DOTNET_PATH` 环境变量,然后是系统级安装。
    若两者都不存在,加载器不会启动,.NET 插件将被跳过(服务器仍正常启动,并打印错误)。
- Windows:VC++ 14.x 运行库(缺失时会报类似 "msvcp140.dll not found")
- Linux:Endstone 原生运行时所需的共享库(例如 `libc++`)

## 安装步骤

1. 解压对应操作系统的发布压缩包,把 `plugins` 与 `plugins.net` 文件夹复制到服务器根目录
   (`bedrock_server` 旁边)。最终目录结构:

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

    注意:`endstone_dotnet_loader.dll` / `.so` 存在两份(`plugins\` 与 `plugins\dotnet_loader\`),
    两者都必需。

2. (可选,但推荐)清空 `plugins\.local\` 文件夹。这是插件加载缓存;升级后遗留的旧缓存
   可能引发问题。

3. 启动服务器。成功启动的输出类似:

    ```
    [DotNetLoader] Loading dotnet_loader
    [DotNetLoader] .NET runtime started.
    [ExamplePlugin] Loading example_plugin v1.0.0
    [DotNetLoader] Loaded 1 .NET plugin(s) from '.../plugins.net'.
    [ExamplePlugin] Enabling example_plugin v1.0.0
    ```

4. 进入游戏输入 `/example hello`(别名 `/ex hello`)。回复 "Hello, <your name>!" 即表示一切正常。

## 验证示例插件

随包附带的 `Example.Plugin.dll` 还提供以下命令:

| 命令 | 用途 |
| --- | --- |
| `/ex test` | 在聊天中发送 `.test` 触发 30 项自动化 API 自检 |
| `/ex whoami` | 显示你 / 控制台的信息 |
| `/ex item` | 查看手中物品的全部属性 |
| `/ex enchant <list\|info\|add\|remove\|clear> [id] [level] [force]` | 手中物品的附魔操作 |
| `/ex tag <show\|hide\|always\|score\|sb>` | 名称标签与计分板标签操作 |
| `/ex mob <type> [name] [health]` | 生成生物 |
| `/ex form <message\|action\|modal>` | 三种表单(仅玩家) |
| `/ex boss <show\|hide>` | boss 血条(仅玩家) |
| `/ex level [time\|block\|highest\|spawn\|drop\|chunks]` | 世界 / 维度操作 |
| `/ex map <create\|send\|item\|clear>` | 地图与地图渲染器 |
| `/ex inv <show\|give\|slot\|clear>` | 背包操作 |
| `/ex sched <once\|async\|timer\|stop\|pending>` | 调度器演示 |

在控制台输入 `dotnet-test` 会触发一条广播,用于验证控制台事件链路。

## 更新与卸载

- **更新**:停止服务器,覆盖 `plugins\` 与 `plugins.net\` 中对应的文件,清空 `plugins\.local\`,重启。
- **卸载**:停止服务器,删除 `plugins\` 下的 `endstone_dotnet_loader.dll`(Windows)/
  `endstone_dotnet_loader.so`(Linux),删除整个 `plugins\dotnet_loader\` 文件夹,
  以及 `plugins.net\` 下对应的 `*.Plugin.dll` 文件,重启。