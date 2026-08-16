# 事件

所有事件类均派生自 `Event`,通过 `PluginBase.RegisterEvent<T>()` 注册。
事件在服务器主线程上同步触发;多数事件暴露 `Player`,并可通过 `IsCancelled` 取消。

| 事件 | 说明 |
| --- | --- |
| `ActorDamageEvent` |  |
| `ActorDeathEvent` |  |
| `ActorExplodeEvent` |  |
| `ActorKnockbackEvent` |  |
| `ActorRemoveEvent` |  |
| `ActorSpawnEvent` |  |
| `ActorTeleportEvent` |  |
| `BlockBreakEvent` |  |
| `BlockCookEvent` |  |
| `BlockExplodeEvent` |  |
| `BlockFormEvent` |  |
| `BlockFromToEvent` |  |
| `BlockGrowEvent` |  |
| `BlockPistonExtendEvent` |  |
| `BlockPistonRetractEvent` |  |
| `BlockPlaceEvent` |  |
| `BroadcastMessageEvent` |  |
| `ChunkLoadEvent` |  |
| `ChunkUnloadEvent` |  |
| `LeavesDecayEvent` |  |
| `MapInitializeEvent` |  |
| `PacketReceiveEvent` |  |
| `PacketSendEvent` |  |
| `PlayerBedEnterEvent` |  |
| `PlayerBedLeaveEvent` |  |
| `PlayerChatEvent` |  |
| `PlayerCommandEvent` |  |
| `PlayerDeathEvent` |  |
| `PlayerDimensionChangeEvent` |  |
| `PlayerDropItemEvent` |  |
| `PlayerEmoteEvent` |  |
| `PlayerGameModeChangeEvent` |  |
| `PlayerInteractActorEvent` |  |
| `PlayerInteractEvent` |  |
| `PlayerItemConsumeEvent` |  |
| `PlayerItemHeldEvent` |  |
| `PlayerJoinEvent` |  |
| `PlayerJumpEvent` |  |
| `PlayerKickEvent` |  |
| `PlayerLoginEvent` |  |
| `PlayerMoveEvent` |  |
| `PlayerPickupItemEvent` |  |
| `PlayerPortalEvent` |  |
| `PlayerQuitEvent` |  |
| `PlayerRespawnEvent` |  |
| `PlayerSkinChangeEvent` |  |
| `PlayerTeleportEvent` |  |
| `PluginDisableEvent` |  |
| `PluginEnableEvent` |  |
| `ScriptMessageEvent` |  |
| `ServerCommandEvent` |  |
| `ServerListPingEvent` |  |
| `ServerLoadEvent` |  |
| `ThunderChangeEvent` |  |
| `WeatherChangeEvent` |  |

## `ActorDamageEvent`

## 属性

### `Damage` : `float`

`{ get;set; }`

### `DamageSource` : `DamageSource`

`{ get; }`

---

## `ActorDeathEvent`

## 属性

### `DamageSource` : `DamageSource`

`{ get; }`

---

## `ActorExplodeEvent`

## 属性

### `BlockCount` : `int`

`{ get; }`

### `Location` : `Location`

`{ get; }`

## 方法

### `Block GetBlock(int index)`

---

## `ActorKnockbackEvent`

## 属性

### `Knockback` : `Location`

`{ get;set; }`

### `Source` : `Actor`

`{ get; }`

---

## `ActorRemoveEvent`

---

## `ActorSpawnEvent`

---

## `ActorTeleportEvent`

## 属性

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `BlockBreakEvent`

---

## `BlockCookEvent`

## 属性

### `Result` : `ItemStack`

`{ get; }`

### `Source` : `ItemStack`

`{ get; }`

---

## `BlockExplodeEvent`

## 属性

### `BlockCount` : `int`

`{ get; }`

## 方法

### `Block GetBlock(int index)`

---

## `BlockFormEvent`

## 属性

### `NewState` : `BlockState`

`{ get; }`

---

## `BlockFromToEvent`

## 属性

### `ToBlock` : `Block`

`{ get; }`

---

## `BlockGrowEvent`

## 属性

### `NewState` : `BlockState`

`{ get; }`

---

## `BlockPistonExtendEvent`

## 属性

### `Direction` : `BlockFace`

`{ get; }`

---

## `BlockPistonRetractEvent`

## 属性

### `Direction` : `BlockFace`

`{ get; }`

---

## `BlockPlaceEvent`

## 属性

### `BlockAgainst` : `Block`

`{ get; }`

### `PlacedState` : `BlockState`

`{ get; }`

---

## `BroadcastMessageEvent`

## 属性

### `Message` : `string`

`{ get;set; }`

### `RecipientCount` : `int`

`{ get; }`

---

## `ChunkLoadEvent`

## 属性

### `DimensionName` : `string`

`{ get; }`

### `X` : `int`

`{ get; }`

### `Z` : `int`

`{ get; }`

---

## `ChunkUnloadEvent`

## 属性

### `DimensionName` : `string`

`{ get; }`

### `X` : `int`

`{ get; }`

### `Z` : `int`

`{ get; }`

---

## `LeavesDecayEvent`

---

## `MapInitializeEvent`

---

## `PacketReceiveEvent`

## 属性

### `Address` : `string`

`{ get; }`

### `PacketId` : `int`

`{ get; }`

### `Payload` : `byte[]`

`{ get;set; }`

### `Player` : `Player`

`{ get; }`

### `SubClientId` : `int`

`{ get; }`

---

## `PacketSendEvent`

## 属性

### `Address` : `string`

`{ get; }`

### `PacketId` : `int`

`{ get; }`

### `Payload` : `byte[]`

`{ get;set; }`

### `Player` : `Player`

`{ get; }`

### `SubClientId` : `int`

`{ get; }`

---

## `PlayerBedEnterEvent`

## 属性

### `Bed` : `Block`

`{ get; }`

---

## `PlayerBedLeaveEvent`

## 属性

### `Bed` : `Block`

`{ get; }`

---

## `PlayerChatEvent`

## 属性

### `Format` : `string`

`{ get;set; }`

### `Message` : `string`

`{ get;set; }`

### `RecipientCount` : `int`

`{ get; }`

---

## `PlayerCommandEvent`

## 属性

### `Command` : `string`

`{ get;set; }`

---

## `PlayerDeathEvent`

## 属性

### `DeathMessage` : `string`

`{ get;set; }`

---

## `PlayerDimensionChangeEvent`

## 属性

### `From` : `string`

`{ get; }`

### `To` : `string`

`{ get; }`

---

## `PlayerDropItemEvent`

## 属性

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerEmoteEvent`

## 属性

### `EmoteId` : `string`

`{ get; }`

### `IsMuted` : `bool`

`{ get; }`

## 方法

### `void SetMuted(bool value)`

---

## `PlayerGameModeChangeEvent`

## 属性

### `NewGameMode` : `GameMode`

`{ get; }`

---

## `PlayerInteractActorEvent`

## 属性

### `Actor` : `Actor`

`{ get; }`

---

## `PlayerInteractEvent`

## 属性

### `Action` : `InteractAction`

`{ get; }`

### `Block` : `Block`

`{ get; }`

### `BlockFace` : `BlockFace`

`{ get; }`

### `ClickedPosition` : `Location?`

`{ get; }`

### `HasBlock` : `bool`

`{ get; }`

### `HasItem` : `bool`

`{ get; }`

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerItemConsumeEvent`

## 属性

### `Hand` : `EquipmentSlot`

`{ get; }`

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerItemHeldEvent`

## 属性

### `NewSlot` : `int`

`{ get; }`

### `PreviousSlot` : `int`

`{ get; }`

---

## `PlayerJoinEvent`

## 属性

### `JoinMessage` : `string`

`{ get;set; }`

---

## `PlayerJumpEvent`

---

## `PlayerKickEvent`

## 属性

### `Reason` : `string`

`{ get;set; }`

---

## `PlayerLoginEvent`

## 属性

### `KickMessage` : `string`

`{ get;set; }`

---

## `PlayerMoveEvent`

## 属性

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PlayerPickupItemEvent`

## 属性

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerPortalEvent`

## 属性

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PlayerQuitEvent`

## 属性

### `QuitMessage` : `string`

`{ get;set; }`

---

## `PlayerRespawnEvent`

---

## `PlayerSkinChangeEvent`

## 属性

### `NewSkinCapeId` : `string`

`{ get; }`

### `NewSkinId` : `string`

`{ get; }`

### `SkinChangeMessage` : `string`

`{ get;set; }`

---

## `PlayerTeleportEvent`

## 属性

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PluginDisableEvent`

## 属性

### `PluginName` : `string`

`{ get; }`

---

## `PluginEnableEvent`

## 属性

### `PluginName` : `string`

`{ get; }`

---

## `ScriptMessageEvent`

## 属性

### `Message` : `string`

`{ get; }`

### `MessageId` : `string`

`{ get; }`

### `SenderName` : `string`

`{ get; }`

---

## `ServerCommandEvent`

## 属性

### `Command` : `string`

`{ get;set; }`

### `Sender` : `CommandSender`

`{ get; }`

### `SenderName` : `string`

`{ get; }`

---

## `ServerListPingEvent`

## 属性

### `Address` : `string`

`{ get; }`

### `GameMode` : `GameMode`

`{ get; }`

### `LevelName` : `string`

`{ get; }`

### `LocalPort` : `int`

`{ get; }`

### `LocalPortV6` : `int`

`{ get; }`

### `MaxPlayers` : `int`

`{ get; }`

### `MinecraftVersionNetwork` : `string`

`{ get; }`

### `Motd` : `string`

`{ get; }`

### `NetworkProtocolVersion` : `int`

`{ get; }`

### `NumPlayers` : `int`

`{ get; }`

### `ServerGuid` : `string`

`{ get; }`

## 方法

### `void SetGameMode(GameMode value)`

### `void SetLevelName(string value)`

### `void SetLocalPort(int value)`

### `void SetLocalPortV6(int value)`

### `void SetMaxPlayers(int value)`

### `void SetMinecraftVersionNetwork(string value)`

### `void SetMotd(string value)`

### `void SetNumPlayers(int value)`

### `void SetServerGuid(string value)`

---

## `ServerLoadEvent`

## 属性

### `Type` : `LoadType`

`{ get; }`

---

## `ThunderChangeEvent`

## 属性

### `ToThunderState` : `bool`

`{ get; }`

---

## `WeatherChangeEvent`

## 属性

### `ToWeatherState` : `bool`

`{ get; }`

---

