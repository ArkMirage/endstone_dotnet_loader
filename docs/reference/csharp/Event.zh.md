# Event

`abstract class`

Base class for event wrappers.

**命名空间** `Endstone.Loader`

**继承** `object`

**派生类** `ActorDamageEvent`, `ActorDeathEvent`, `ActorExplodeEvent`, `ActorKnockbackEvent`, `ActorRemoveEvent`, `ActorSpawnEvent`, `ActorTeleportEvent`, `BlockBreakEvent`, `BlockCookEvent`, `BlockExplodeEvent`, `BlockFormEvent`, `BlockFromToEvent`, `BlockGrowEvent`, `BlockPistonExtendEvent`, `BlockPistonRetractEvent`, `BlockPlaceEvent`, `BroadcastMessageEvent`, `ChunkLoadEvent`, `ChunkUnloadEvent`, `LeavesDecayEvent`, `MapInitializeEvent`, `PacketReceiveEvent`, `PacketSendEvent`, `PlayerBedEnterEvent`, `PlayerBedLeaveEvent`, `PlayerChatEvent`, `PlayerCommandEvent`, `PlayerDeathEvent`, `PlayerDimensionChangeEvent`, `PlayerDropItemEvent`, `PlayerEmoteEvent`, `PlayerGameModeChangeEvent`, `PlayerInteractActorEvent`, `PlayerInteractEvent`, `PlayerItemConsumeEvent`, `PlayerItemHeldEvent`, `PlayerJoinEvent`, `PlayerJumpEvent`, `PlayerKickEvent`, `PlayerLoginEvent`, `PlayerMoveEvent`, `PlayerPickupItemEvent`, `PlayerPortalEvent`, `PlayerQuitEvent`, `PlayerRespawnEvent`, `PlayerSkinChangeEvent`, `PlayerTeleportEvent`, `PluginDisableEvent`, `PluginEnableEvent`, `ScriptMessageEvent`, `ServerCommandEvent`, `ServerListPingEvent`, `ServerLoadEvent`, `ThunderChangeEvent`, `WeatherChangeEvent`

## 属性

### `Actor` : `Actor`

`{ get; }`

### `IsCancelled` : `bool`

`{ get;set; }`

### `Player` : `Player`

`{ get; }`

## 方法

### `void Cancel()`

