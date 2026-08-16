# Actor

`class`

包装原生 endstone::Actor 的托管对象。实体是世界中可移动、可交互的对象(玩家、生物、掉落物等)。

**命名空间** `Endstone.Loader`

**继承** `object`

**派生类** `Mob`, `Player`

## 属性

### `DimensionName` : `string`

`{ get; }`

### `Id` : `long`

`{ get; }`

实体的唯一 ID。

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

实体当前的位置(坐标 + 视角)。

### `Name` : `string`

`{ get; }`

实体的名称。

### `NameTag` : `string`

`{ get; }`

### `RuntimeId` : `ulong`

`{ get; }`

实体的运行时 ID,每条运行时唯一。

### `ScoreTag` : `string`

`{ get; }`

### `ScoreboardTagCount` : `int`

`{ get; }`

### `Type` : `string`

`{ get; }`

实体的类型标识(例如 "minecraft:zombie")。

### `Velocity` : `Location`

`{ get; }`

实体当前的速度向量。

## 方法

### `bool AddScoreboardTag(string tag)`

### `Mob AsMob()`

### `Block GetBlock(int x, int y, int z)`

Gets the block at the given block coordinates in this actor's dimension.

### `string GetScoreboardTag(int index)`

### `void Remove()`

### `bool RemoveScoreboardTag(string tag)`

### `void SendMessage(string message)`

向实体发送聊天消息。

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

将实体传送到另一实体的位置。

### `bool Teleport(Location location)`

将实体传送至同一维度内指定位置。

