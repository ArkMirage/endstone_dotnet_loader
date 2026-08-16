# Block

`sealed class`

Wraps a native endstone::Block.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Dimension` : `string`

`{ get; }`

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

## Methods

### `BlockState CaptureState()`

### `void Dispose()`

### `Block GetRelative(int offsetX, int offsetY, int offsetZ)`

Gets the block at the given offsets. Caller owns the returned block (Dispose it).

### `void SetType(string type)`

### `void SetType(string type, bool applyPhysics)`

