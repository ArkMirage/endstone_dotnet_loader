# BossBar

`sealed class`

A boss bar displayed to players at the top of the screen. Created via Server.CreateBossBar; Dispose() frees the native object (bar disappears).

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Color` : `BarColor`

`{ get;set; }`

### `IsVisible` : `bool`

`{ get;set; }`

### `Players` : `Player[]`

`{ get; }`

### `Progress` : `float`

`{ get;set; }`

### `Style` : `BarStyle`

`{ get;set; }`

### `Title` : `string`

`{ get;set; }`

## Methods

### `void AddFlag(BarFlag flag)`

### `void AddPlayer(Player player)`

### `void Dispose()`

### `bool HasFlag(BarFlag flag)`

### `void RemoveAll()`

### `void RemoveFlag(BarFlag flag)`

### `void RemovePlayer(Player player)`

