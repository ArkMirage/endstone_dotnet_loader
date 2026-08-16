# Player

`sealed class`

Wrapper around a native endstone::Player (also an Actor).

**Namespace** `Endstone.Loader`

**Inheritance** `Actor` › `object`

## Properties

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

### `ExpLevel` : `int`

`{ get; }`

### `ExpProgress` : `float`

`{ get; }`

### `FlySpeed` : `float`

`{ get; }`

### `GameMode` : `GameMode`

`{ get; }`

### `GameVersion` : `string`

`{ get; }`

### `Inventory` : `PlayerInventory`

`{ get; }`

### `IsFlying` : `bool`

`{ get; }`

### `IsOp` : `bool`

`{ get; }`

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

## Methods

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

