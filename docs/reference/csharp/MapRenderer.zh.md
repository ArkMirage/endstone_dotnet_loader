# MapRenderer

`abstract class`

地图渲染器基类:重写 Render() 在 MapCanvas 上绘制,通过 MapView.AddRenderer 添加。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `IsContextual` : `bool`

`{ get; }`

### `Map` : `MapView`

`{ get; }`

## 方法

### `void Render(MapView map, MapCanvas canvas, Player player)`

Renders one frame of the map for the given player.

