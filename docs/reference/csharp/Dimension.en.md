# Dimension

`sealed class`

Wraps a native endstone::Dimension.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Level` : `Level`

`{ get; }`

### `Name` : `string`

`{ get; }`

### `Type` : `DimensionType`

`{ get; }`

## Methods

### `ItemStack DropItem(Location location, ItemStack item)`

Drops the item stack at the location. The returned wrapper reads the resulting item entity (call RemoveFromWorld after pickup-related cleanup no longer needed).

### `Actor[] GetActors()`

### `Block GetBlockAt(int x, int y, int z)`

Gets the block at the given block coordinates. Caller owns the returned block (Dispose it).

### `Block GetHighestBlockAt(int x, int z)`

Gets the highest block at the given coordinates. Caller owns the returned block (Dispose it).

### `int GetHighestBlockYAt(int x, int z)`

### `Chunk[] GetLoadedChunks()`

### `Actor SpawnActor(string type, Location location)`

Spawns an actor of the given type (e.g. "minecraft:zombie") at the location. Returns null if failed.

