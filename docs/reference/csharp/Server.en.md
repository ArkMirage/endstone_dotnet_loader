# Server

`sealed class`

Wrapper around the native endstone::Server singleton.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Level` : `Level`

`{ get; }`

### `MaxPlayers` : `int`

`{ get; }`

### `MinecraftVersion` : `string`

`{ get; }`

### `Name` : `string`

`{ get; }`

### `ProtocolVersion` : `int`

`{ get; }`

### `Version` : `string`

`{ get; }`

## Methods

### `void BroadcastMessage(string message)`

### `BossBar CreateBossBar(string title, BarColor color, BarStyle style, BarFlag flags)`

Creates a boss bar (progress defaults to 1.0, visible by default).

### `MapView CreateMap(Dimension dimension)`

Creates a new map view (automatically assigned ID) for the given dimension.

### `bool DispatchCommand(string commandLine)`

### `MapView GetMap(long id)`

Gets the map view with the given item ID, or null if it does not exist.

### `Player[] GetOnlinePlayers()`

### `Player GetPlayer(string name)`

