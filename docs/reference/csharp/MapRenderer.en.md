# MapRenderer

`abstract class`

Base class for plugin map renderers. Subclass and override Render to draw onto the canvas.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `IsContextual` : `bool`

`{ get; }`

### `Map` : `MapView`

`{ get; }`

## Methods

### `void Render(MapView map, MapCanvas canvas, Player player)`

Renders one frame of the map for the given player.

