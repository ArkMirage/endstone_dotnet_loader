# Actor

`class`

Wraps a native endstone::Actor.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

**Derived classes** `Mob`, `Player`

## Properties

### `DimensionName` : `string`

`{ get; }`

### `Id` : `long`

`{ get; }`

### `IsDead` : `bool`

`{ get; }`

### `IsInLava` : `bool`

`{ get; }`

### `IsInWater` : `bool`

`{ get; }`

### `IsNameTagAlwaysVisible` : `bool`

`{ get; }`

### `IsNameTagVisible` : `bool`

`{ get; }`

### `IsOnGround` : `bool`

`{ get; }`

### `IsValid` : `bool`

`{ get; }`

### `Location` : `Location`

`{ get; }`

### `Name` : `string`

`{ get; }`

### `NameTag` : `string`

`{ get; }`

### `RuntimeId` : `ulong`

`{ get; }`

### `ScoreTag` : `string`

`{ get; }`

### `ScoreboardTagCount` : `int`

`{ get; }`

### `Type` : `string`

`{ get; }`

### `Velocity` : `Location`

`{ get; }`

## Methods

### `bool AddScoreboardTag(string tag)`

### `Mob AsMob()`

### `Block GetBlock(int x, int y, int z)`

Gets the block at the given block coordinates in this actor's dimension.

### `string GetScoreboardTag(int index)`

### `void Remove()`

### `bool RemoveScoreboardTag(string tag)`

### `void SendMessage(string message)`

### `void SendMessage(string format, params object[] args)`

### `void SetNameTag(string nameTag)`

### `void SetNameTagAlwaysVisible(bool visible)`

### `void SetNameTagVisible(bool visible)`

### `void SetRotation(float yaw, float pitch)`

### `void SetScoreTag(string scoreTag)`

### `Actor SpawnActor(string type, Location? location)`

Spawns an actor of the given type (e.g. "minecraft:zombie") at a location in this actor's dimension. Returns null if spawning failed.

### `Mob SpawnMob(string type, Location? location)`

Spawns a mob and returns it, or null if it is not a mob / failed to spawn.

### `bool Teleport(Actor target)`

Teleports this actor to another actor's position.

### `bool Teleport(Location location)`

Teleports this actor to the given location (same dimension).

