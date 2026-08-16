# Events

All event classes derive from `Event`. Register handlers with `PluginBase.RegisterEvent<T>()`. Events are raised synchronously on the server thread; most expose a `Player` and cancellation via `IsCancelled`.

| Event | Summary |
| --- | --- |
| [`ActorDamageEvent`](#actordamageevent) |  |
| [`ActorDeathEvent`](#actordeathevent) |  |
| [`ActorExplodeEvent`](#actorexplodeevent) |  |
| [`ActorKnockbackEvent`](#actorknockbackevent) |  |
| [`ActorRemoveEvent`](#actorremoveevent) |  |
| [`ActorSpawnEvent`](#actorspawnevent) |  |
| [`ActorTeleportEvent`](#actorteleportevent) |  |
| [`BlockBreakEvent`](#blockbreakevent) |  |
| [`BlockCookEvent`](#blockcookevent) |  |
| [`BlockExplodeEvent`](#blockexplodeevent) |  |
| [`BlockFormEvent`](#blockformevent) |  |
| [`BlockFromToEvent`](#blockfromtoevent) |  |
| [`BlockGrowEvent`](#blockgrowevent) |  |
| [`BlockPistonExtendEvent`](#blockpistonextendevent) |  |
| [`BlockPistonRetractEvent`](#blockpistonretractevent) |  |
| [`BlockPlaceEvent`](#blockplaceevent) |  |
| [`BroadcastMessageEvent`](#broadcastmessageevent) |  |
| [`ChunkLoadEvent`](#chunkloadevent) |  |
| [`ChunkUnloadEvent`](#chunkunloadevent) |  |
| [`LeavesDecayEvent`](#leavesdecayevent) |  |
| [`MapInitializeEvent`](#mapinitializeevent) |  |
| [`PacketReceiveEvent`](#packetreceiveevent) |  |
| [`PacketSendEvent`](#packetsendevent) |  |
| [`PlayerBedEnterEvent`](#playerbedenterevent) |  |
| [`PlayerBedLeaveEvent`](#playerbedleaveevent) |  |
| [`PlayerChatEvent`](#playerchatevent) |  |
| [`PlayerCommandEvent`](#playercommandevent) |  |
| [`PlayerDeathEvent`](#playerdeathevent) |  |
| [`PlayerDimensionChangeEvent`](#playerdimensionchangeevent) |  |
| [`PlayerDropItemEvent`](#playerdropitemevent) |  |
| [`PlayerEmoteEvent`](#playeremoteevent) |  |
| [`PlayerGameModeChangeEvent`](#playergamemodechangeevent) |  |
| [`PlayerInteractActorEvent`](#playerinteractactorevent) |  |
| [`PlayerInteractEvent`](#playerinteractevent) |  |
| [`PlayerItemConsumeEvent`](#playeritemconsumeevent) |  |
| [`PlayerItemHeldEvent`](#playeritemheldevent) |  |
| [`PlayerJoinEvent`](#playerjoinevent) |  |
| [`PlayerJumpEvent`](#playerjumpevent) |  |
| [`PlayerKickEvent`](#playerkickevent) |  |
| [`PlayerLoginEvent`](#playerloginevent) |  |
| [`PlayerMoveEvent`](#playermoveevent) |  |
| [`PlayerPickupItemEvent`](#playerpickupitemevent) |  |
| [`PlayerPortalEvent`](#playerportalevent) |  |
| [`PlayerQuitEvent`](#playerquitevent) |  |
| [`PlayerRespawnEvent`](#playerrespawnevent) |  |
| [`PlayerSkinChangeEvent`](#playerskinchangeevent) |  |
| [`PlayerTeleportEvent`](#playerteleportevent) |  |
| [`PluginDisableEvent`](#plugindisableevent) |  |
| [`PluginEnableEvent`](#pluginenableevent) |  |
| [`ScriptMessageEvent`](#scriptmessageevent) |  |
| [`ServerCommandEvent`](#servercommandevent) |  |
| [`ServerListPingEvent`](#serverlistpingevent) |  |
| [`ServerLoadEvent`](#serverloadevent) |  |
| [`ThunderChangeEvent`](#thunderchangeevent) |  |
| [`WeatherChangeEvent`](#weatherchangeevent) |  |

## `ActorDamageEvent`

## Properties

### `Damage` : `float`

`{ get;set; }`

### `DamageSource` : `DamageSource`

`{ get; }`

---

## `ActorDeathEvent`

## Properties

### `DamageSource` : `DamageSource`

`{ get; }`

---

## `ActorExplodeEvent`

## Properties

### `BlockCount` : `int`

`{ get; }`

### `Location` : `Location`

`{ get; }`

## Methods

### `Block GetBlock(int index)`

---

## `ActorKnockbackEvent`

## Properties

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

## Properties

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `BlockBreakEvent`

---

## `BlockCookEvent`

## Properties

### `Result` : `ItemStack`

`{ get; }`

### `Source` : `ItemStack`

`{ get; }`

---

## `BlockExplodeEvent`

## Properties

### `BlockCount` : `int`

`{ get; }`

## Methods

### `Block GetBlock(int index)`

---

## `BlockFormEvent`

## Properties

### `NewState` : `BlockState`

`{ get; }`

---

## `BlockFromToEvent`

## Properties

### `ToBlock` : `Block`

`{ get; }`

---

## `BlockGrowEvent`

## Properties

### `NewState` : `BlockState`

`{ get; }`

---

## `BlockPistonExtendEvent`

## Properties

### `Direction` : `BlockFace`

`{ get; }`

---

## `BlockPistonRetractEvent`

## Properties

### `Direction` : `BlockFace`

`{ get; }`

---

## `BlockPlaceEvent`

## Properties

### `BlockAgainst` : `Block`

`{ get; }`

### `PlacedState` : `BlockState`

`{ get; }`

---

## `BroadcastMessageEvent`

## Properties

### `Message` : `string`

`{ get;set; }`

### `RecipientCount` : `int`

`{ get; }`

---

## `ChunkLoadEvent`

## Properties

### `DimensionName` : `string`

`{ get; }`

### `X` : `int`

`{ get; }`

### `Z` : `int`

`{ get; }`

---

## `ChunkUnloadEvent`

## Properties

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

## Properties

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

## Properties

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

## Properties

### `Bed` : `Block`

`{ get; }`

---

## `PlayerBedLeaveEvent`

## Properties

### `Bed` : `Block`

`{ get; }`

---

## `PlayerChatEvent`

## Properties

### `Format` : `string`

`{ get;set; }`

### `Message` : `string`

`{ get;set; }`

### `RecipientCount` : `int`

`{ get; }`

---

## `PlayerCommandEvent`

## Properties

### `Command` : `string`

`{ get;set; }`

---

## `PlayerDeathEvent`

## Properties

### `DeathMessage` : `string`

`{ get;set; }`

---

## `PlayerDimensionChangeEvent`

## Properties

### `From` : `string`

`{ get; }`

### `To` : `string`

`{ get; }`

---

## `PlayerDropItemEvent`

## Properties

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerEmoteEvent`

## Properties

### `EmoteId` : `string`

`{ get; }`

### `IsMuted` : `bool`

`{ get; }`

## Methods

### `void SetMuted(bool value)`

---

## `PlayerGameModeChangeEvent`

## Properties

### `NewGameMode` : `GameMode`

`{ get; }`

---

## `PlayerInteractActorEvent`

## Properties

### `Actor` : `Actor`

`{ get; }`

---

## `PlayerInteractEvent`

## Properties

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

## Properties

### `Hand` : `EquipmentSlot`

`{ get; }`

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerItemHeldEvent`

## Properties

### `NewSlot` : `int`

`{ get; }`

### `PreviousSlot` : `int`

`{ get; }`

---

## `PlayerJoinEvent`

## Properties

### `JoinMessage` : `string`

`{ get;set; }`

---

## `PlayerJumpEvent`

---

## `PlayerKickEvent`

## Properties

### `Reason` : `string`

`{ get;set; }`

---

## `PlayerLoginEvent`

## Properties

### `KickMessage` : `string`

`{ get;set; }`

---

## `PlayerMoveEvent`

## Properties

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PlayerPickupItemEvent`

## Properties

### `Item` : `ItemStack`

`{ get; }`

---

## `PlayerPortalEvent`

## Properties

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PlayerQuitEvent`

## Properties

### `QuitMessage` : `string`

`{ get;set; }`

---

## `PlayerRespawnEvent`

---

## `PlayerSkinChangeEvent`

## Properties

### `NewSkinCapeId` : `string`

`{ get; }`

### `NewSkinId` : `string`

`{ get; }`

### `SkinChangeMessage` : `string`

`{ get;set; }`

---

## `PlayerTeleportEvent`

## Properties

### `From` : `Location`

`{ get;set; }`

### `To` : `Location`

`{ get;set; }`

---

## `PluginDisableEvent`

## Properties

### `PluginName` : `string`

`{ get; }`

---

## `PluginEnableEvent`

## Properties

### `PluginName` : `string`

`{ get; }`

---

## `ScriptMessageEvent`

## Properties

### `Message` : `string`

`{ get; }`

### `MessageId` : `string`

`{ get; }`

### `SenderName` : `string`

`{ get; }`

---

## `ServerCommandEvent`

## Properties

### `Command` : `string`

`{ get;set; }`

### `Sender` : `CommandSender`

`{ get; }`

### `SenderName` : `string`

`{ get; }`

---

## `ServerListPingEvent`

## Properties

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

## Methods

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

## Properties

### `Type` : `LoadType`

`{ get; }`

---

## `ThunderChangeEvent`

## Properties

### `ToThunderState` : `bool`

`{ get; }`

---

## `WeatherChangeEvent`

## Properties

### `ToWeatherState` : `bool`

`{ get; }`

---

