# MapCanvas

`sealed class`

地图画布:通过 SetPixel/SetPixelColor 逐像素绘制地图渲染内容。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Map` : `MapView`

`{ get; }`

## 方法

### `uint GetBasePixel(int x, int y)`

### `MapColor GetBasePixelColor(int x, int y)`

### `MapCursor[] GetCursors()`

### `uint GetPixel(int x, int y)`

### `MapColor GetPixelColor(int x, int y)`

### `void SetCursors(System.Collections.Generic.IEnumerable<MapCursor> cursors)`

### `void SetPixel(int x, int y, uint color)`

### `void SetPixelColor(int x, int y, MapColor color)`

