# MapView

`sealed class`

包装原生 endstone::MapView 的托管对象。服务器通过 CreateMap 创建地图视图,可调整缩放、中心与跟踪设置,并添加渲染器。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `CenterX` : `int`

`{ get;set; }`

### `CenterZ` : `int`

`{ get;set; }`

### `Dimension` : `Dimension`

`{ get; }`

### `Id` : `long`

`{ get; }`

### `IsLocked` : `bool`

`{ get;set; }`

### `IsUnlimitedTracking` : `bool`

`{ get;set; }`

### `IsVirtual` : `bool`

`{ get; }`

### `Renderers` : `MapRenderer[]`

`{ get; }`

### `Scale` : `MapScale`

`{ get;set; }`

## 方法

### `void AddRenderer(MapRenderer renderer)`

Attaches a renderer. The renderer draws on the canvas whenever this map is rendered for a player.

### `bool RemoveRenderer(MapRenderer renderer)`

Detaches a renderer. Returns true if it was attached to this map.

### `void SetDimension(Dimension dimension)`

