# Event

`abstract class`

Base class for event wrappers.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

**Derived classes** `ActorDamageEvent`, `ActorDeathEvent`, `ActorExplodeEvent`, `ActorKnockbackEvent`, `ActorRemoveEvent`, `ActorSpawnEvent`, `ActorTeleportEvent`, `BlockBreakEvent`, `BlockCookEvent`, `BlockExplodeEvent`, `BlockFormEvent`, `BlockFromToEvent`, `BlockGrowEvent`, `BlockPistonExtendEvent`, `BlockPistonRetractEvent`, `BlockPlaceEvent`, `BroadcastMessageEvent`, `ChunkLoadEvent`, `ChunkUnloadEvent`, `LeavesDecayEvent`, `MapInitializeEvent`, `PacketReceiveEvent`, `PacketSendEvent`, `PlayerBedEnterEvent`, `PlayerBedLeaveEvent`, `PlayerChatEvent`, `PlayerCommandEvent`, `PlayerDeathEvent`, `PlayerDimensionChangeEvent`, `PlayerDropItemEvent`, `PlayerEmoteEvent`, `PlayerGameModeChangeEvent`, `PlayerInteractActorEvent`, `PlayerInteractEvent`, `PlayerItemConsumeEvent`, `PlayerItemHeldEvent`, `PlayerJoinEvent`, `PlayerJumpEvent`, `PlayerKickEvent`, `PlayerLoginEvent`, `PlayerMoveEvent`, `PlayerPickupItemEvent`, `PlayerPortalEvent`, `PlayerQuitEvent`, `PlayerRespawnEvent`, `PlayerSkinChangeEvent`, `PlayerTeleportEvent`, `PluginDisableEvent`, `PluginEnableEvent`, `ScriptMessageEvent`, `ServerCommandEvent`, `ServerListPingEvent`, `ServerLoadEvent`, `ThunderChangeEvent`, `WeatherChangeEvent`

## Properties

### `Actor` : `Actor`

`{ get; }`

### `IsCancelled` : `bool`

`{ get;set; }`

### `Player` : `Player`

`{ get; }`

## Methods

### `void Cancel()`

