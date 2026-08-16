# BossBar

`sealed class`

包装原生 endstone::BossBar 的托管对象。Boss 血条可以添加进度的文字说明与颜色/分段样式,并绑定到多个玩家。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Color` : `BarColor`

`{ get;set; }`

boss 血条的颜色。

### `IsVisible` : `bool`

`{ get;set; }`

boss 血条是否可见。

### `Players` : `Player[]`

`{ get; }`

### `Progress` : `float`

`{ get;set; }`

boss 血条的进度(0.0 - 1.0)。

### `Style` : `BarStyle`

`{ get;set; }`

boss 血条的分段样式。

### `Title` : `string`

`{ get;set; }`

boss 血条的标题文字。

## 方法

### `void AddFlag(BarFlag flag)`

添加一个标志(例如暗灭天空)。

### `void AddPlayer(Player player)`

将玩家加入可见列表。

### `void Dispose()`

### `bool HasFlag(BarFlag flag)`

### `void RemoveAll()`

### `void RemoveFlag(BarFlag flag)`

### `void RemovePlayer(Player player)`

将玩家从可见列表移除。

