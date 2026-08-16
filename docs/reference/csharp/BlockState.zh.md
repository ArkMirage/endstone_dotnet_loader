# BlockState

`sealed class`

方块状态的快照,可独立修改再更新到世界中,用于实现原子化的方块改动。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Location` : `Location`

`{ get; }`

### `Type` : `string`

`{ get; }`

### `X` : `int`

`{ get; }`

### `Y` : `int`

`{ get; }`

### `Z` : `int`

`{ get; }`

## 方法

### `void Dispose()`

### `void SetType(string type)`

### `bool Update()`

### `bool Update(bool force)`

带外力标志的状态更新。

### `bool Update(bool force, bool applyPhysics)`

