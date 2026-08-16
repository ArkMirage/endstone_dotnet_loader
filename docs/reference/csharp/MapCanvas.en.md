# MapCanvas

`sealed class`

Wraps a native endstone::MapCanvas (valid only during a Render call).

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Map` : `MapView`

`{ get; }`

## Methods

### `uint GetBasePixel(int x, int y)`

### `MapColor GetBasePixelColor(int x, int y)`

### `MapCursor[] GetCursors()`

### `uint GetPixel(int x, int y)`

### `MapColor GetPixelColor(int x, int y)`

### `void SetCursors(System.Collections.Generic.IEnumerable<MapCursor> cursors)`

### `void SetPixel(int x, int y, uint color)`

### `void SetPixelColor(int x, int y, MapColor color)`

