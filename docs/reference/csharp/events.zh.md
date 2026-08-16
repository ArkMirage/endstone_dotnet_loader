# 事件

所有事件类均派生自 `Event`,通过 `PluginBase.RegisterEvent<T>()` 注册。
事件在服务器主线程上同步触发;多数事件暴露 `Player`,并可通过 `IsCancelled` 取消。

在英文版中查看完整成员列表。

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

