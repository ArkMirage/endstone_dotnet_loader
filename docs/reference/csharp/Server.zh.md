# Server

`sealed class`

包装原生 endstone::Server 单例的托管对象,是访问服务器信息、广播、玩家、地图、boss 血条与世界的入口。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Level` : `Level`

`{ get; }`

服务器世界,未加载完成时为 null。

### `MaxPlayers` : `int`

`{ get; }`

### `MinecraftVersion` : `string`

`{ get; }`

Minecraft 版本(如 1.21.x)。

### `Name` : `string`

`{ get; }`

服务器名称。

### `ProtocolVersion` : `int`

`{ get; }`

### `Version` : `string`

`{ get; }`

服务器版本。

## 方法

### `void BroadcastMessage(string message)`

向服务器广播一条消息。

### `BossBar CreateBossBar(string title, BarColor color, BarStyle style, BarFlag flags)`

Creates a boss bar (progress defaults to 1.0, visible by default).

### `MapView CreateMap(Dimension dimension)`

在指定维度创建地图视图。

### `bool DispatchCommand(string commandLine)`

以控制台身份执行一条命令。

### `MapView GetMap(long id)`

Gets the map view with the given item ID, or null if it does not exist.

### `Player[] GetOnlinePlayers()`

### `Player GetPlayer(string name)`

按名称查找在线玩家,不存在返回 null。

