# Level

`sealed class`

包装原生 endstone::Level 的托管对象,表示服务器世界。可读写时间、获取种子、查询维度与实体。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Name` : `string`

`{ get; }`

世界的名称。

### `Seed` : `long`

`{ get; }`

### `Time` : `int`

`{ get;set; }`

当前的世界时间(tick)。

## 方法

### `Actor[] GetActors()`

### `Dimension GetDimension(string name)`

### `Dimension[] GetDimensions()`

