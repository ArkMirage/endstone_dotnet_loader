# Player

`sealed class`

包装原生 endstone::Player 的托管对象(继承 Actor),提供消息、操作、物品、模式、飞行、传送等玩家功能。

**命名空间** `Endstone.Loader`

**继承** `Actor` › `object`

## 属性

### `Address` : `string`

`{ get; }`

### `AllowFlight` : `bool`

`{ get; }`

### `DeviceId` : `string`

`{ get; }`

### `DeviceOS` : `string`

`{ get; }`

### `EnderChest` : `Inventory`

`{ get; }`

玩家末影箱。

### `ExpLevel` : `int`

`{ get; }`

### `ExpProgress` : `float`

`{ get; }`

### `FlySpeed` : `float`

`{ get; }`

### `GameMode` : `GameMode`

`{ get; }`

玩家的游戏模式。

### `GameVersion` : `string`

`{ get; }`

### `Inventory` : `PlayerInventory`

`{ get; }`

玩家主手背包。

### `IsFlying` : `bool`

`{ get; }`

### `IsOp` : `bool`

`{ get; }`

玩家是否为服务器管理员 (OP)。

### `IsSneaking` : `bool`

`{ get; }`

### `IsSprinting` : `bool`

`{ get; }`

### `ItemInHand` : `ItemStack`

`{ get; }`

### `Locale` : `string`

`{ get; }`

### `Name` : `string`

`{ get; }`

### `Ping` : `int`

`{ get; }`

### `SkinCapeId` : `string`

`{ get; }`

### `SkinId` : `string`

`{ get; }`

### `TotalExp` : `int`

`{ get; }`

### `WalkSpeed` : `float`

`{ get; }`

### `Xuid` : `string`

`{ get; }`

## 方法

### `void CloseForm()`

### `void GiveExp(int amount)`

### `void GiveExpLevels(int amount)`

### `void Kick(string reason)`

### `bool PerformCommand(string command)`

### `void PlaySound(Location location, string sound, float volume, float pitch)`

### `void ResetTitle()`

### `void SendErrorMessage(string message)`

### `void SendErrorMessage(string format, params object[] args)`

### `void SendMap(MapView map)`

Sends the full map rendering (pixels + cursors) to this player. Blocks the server thread while the renderers draw.

### `void SendMessage(string message)`

### `void SendMessage(string format, params object[] args)`

### `void SendPacket(int packetId, System.ReadOnlySpan<byte> payload)`

### `void SendPopup(string message)`

### `void SendTip(string message)`

向玩家发送 Tip 悬浮提示。

### `void SendTitle(string title, string subtitle)`

### `void SendToast(string title, string content)`

### `void SetAllowFlight(bool value)`

### `void SetExpLevel(int level)`

### `void SetExpProgress(float progress)`

### `void SetFlySpeed(float value)`

### `void SetFlying(bool value)`

### `void SetGameMode(GameMode mode)`

### `void SetOp(bool value)`

### `void SetSneaking(bool value)`

### `void SetSprinting(bool value)`

### `void SetWalkSpeed(float value)`

### `void ShowForm(FormBase<T> form)`

### `void SpawnParticle(string name, Location location)`

### `void SpawnParticle(string name, Location location, string molangVariablesJson)`

### `void StopAllSounds()`

### `void StopSound(string sound)`

### `void Transfer(string host, int port)`

### `void UpdateCommands()`

