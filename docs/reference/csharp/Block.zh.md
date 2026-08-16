# Block

`sealed class`

包装原生 endstone::Block 的托管对象,表示世界中的方块实例。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Dimension` : `string`

`{ get; }`

### `Location` : `Location`

`{ get; }`

### `Type` : `string`

`{ get; }`

方块的类型标识(例如 "minecraft:stone")。

### `X` : `int`

`{ get; }`

### `Y` : `int`

`{ get; }`

### `Z` : `int`

`{ get; }`

## 方法

### `BlockState CaptureState()`

### `void Dispose()`

### `Block GetRelative(int offsetX, int offsetY, int offsetZ)`

Gets the block at the given offsets. Caller owns the returned block (Dispose it).

### `void SetType(string type)`

设置方块的类型。

### `void SetType(string type, bool applyPhysics)`

