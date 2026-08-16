# MapView

`sealed class`

Wraps a native endstone::MapView (server-owned; never dispose).

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

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

## Methods

### `void AddRenderer(MapRenderer renderer)`

Attaches a renderer. The renderer draws on the canvas whenever this map is rendered for a player.

### `bool RemoveRenderer(MapRenderer renderer)`

Detaches a renderer. Returns true if it was attached to this map.

### `void SetDimension(Dimension dimension)`

