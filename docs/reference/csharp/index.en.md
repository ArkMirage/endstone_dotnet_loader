# Endstone.DotNet Loader API Reference

Every page below is **generated from source**: the code generator (`tools/DocGen`) reflects over `Endstone.Loader.dll`, merges the XML doc comments from `src/csharp`, and renders these Markdown pages. Never edit them by hand - run the generator instead.

## Core

| Type | Summary |
| --- | --- |
| [`Server`](Server.md) | Wrapper around the native endstone::Server singleton. |
| [`PluginBase`](PluginBase.md) | Base class for all .NET Endstone plugins. |
| [`PluginAttribute`](PluginAttribute.md) | Attribute describing plugin metadata. The plugin name must contain only lowercase letters, numbers and underscores (E... |
| [`Logger`](Logger.md) | Logger bound to a plugin's native Endstone logger. |
| [`Scheduler`](Scheduler.md) | Plugin-scoped facade over the native endstone Scheduler. Tasks are owned by the plugin, so all pending tasks are canc... |
| [`ScheduledTask`](ScheduledTask.md) | Handle for a task queued on the native endstone scheduler. Sync tasks run on the server thread, async tasks on a sche... |
| [`Service`](Service.md) | Base class for services registered with the server's service manager (mirrors endstone::Service). A service is a mark... |
| [`ServiceManager`](ServiceManager.md) | Plugin-scoped facade over the server's native service manager (mirrors endstone::ServiceManager). Registering a provi... |
| [`CommandSender`](CommandSender.md) | Wraps a native endstone::CommandSender. |
| [`CommandBuilder`](CommandBuilder.md) | Fluent builder that declares a plugin command. |

## Entities

| Type | Summary |
| --- | --- |
| [`Actor`](Actor.md) | Wraps a native endstone::Actor. |
| [`Player`](Player.md) | Wrapper around a native endstone::Player (also an Actor). |
| [`Mob`](Mob.md) | Wraps a native endstone::Mob (an Actor with health). |
| [`DamageSource`](DamageSource.md) | Wraps a native endstone::DamageSource. |
| [`Enchantment`](Enchantment.md) | Wraps a native endstone::Enchantment registry entry. Instances are transient views of server-owned objects; do not st... |
| [`ItemEnchantment`](ItemEnchantment.md) | An enchantment applied to an item stack, paired with its level. |

## World

| Type | Summary |
| --- | --- |
| [`Level`](Level.md) | Wraps a native endstone::Level. |
| [`Dimension`](Dimension.md) | Wraps a native endstone::Dimension. |
| [`Chunk`](Chunk.md) | Wraps a native endstone::Chunk. Owns the native chunk when created from GetLoadedChunks. |
| [`Block`](Block.md) | Wraps a native endstone::Block. |
| [`BlockState`](BlockState.md) | Wraps a native endstone::BlockState. |
| [`Location`](Location.md) |  |

## Items & Inventory

| Type | Summary |
| --- | --- |
| [`ItemStack`](ItemStack.md) | Wraps a native endstone::ItemStack read-only view. When created via ItemStack.Create, Dispose() frees the native stac... |
| [`Inventory`](Inventory.md) | Wraps a native endstone::Inventory (server-owned, never deleted). Item snapshots returned by getters are transient co... |
| [`PlayerInventory`](PlayerInventory.md) | Wraps a native endstone::PlayerInventory (player's 36 slots + armor + hands). |

## UI

| Type | Summary |
| --- | --- |
| [`FormBase`1`](FormBase.md) | Fluent base for the three form kinds. Send() hands ownership to the native side (the holder is freed after send) and ... |
| [`MessageForm`](MessageForm.md) | A simple form with a title, content and two buttons. |
| [`ActionForm`](ActionForm.md) | A form with a title, content and any number of buttons. |
| [`ModalForm`](ModalForm.md) | A custom form with typed controls and a submit button. The submit payload is the raw JSON response array (values in c... |
| [`BossBar`](BossBar.md) | A boss bar displayed to players at the top of the screen. Created via Server.CreateBossBar; Dispose() frees the nativ... |
| [`MapView`](MapView.md) | Wraps a native endstone::MapView (server-owned; never dispose). |
| [`MapCanvas`](MapCanvas.md) | Wraps a native endstone::MapCanvas (valid only during a Render call). |
| [`MapCursor`](MapCursor.md) | A marker shown on a map. |
| [`MapColor`](MapColor.md) | RGBA color used for map canvas pixels. |
| [`MapRenderer`](MapRenderer.md) | Base class for plugin map renderers. Subclass and override Render to draw onto the canvas. |

## Native Interop

| Type | Summary |
| --- | --- |
| [`Bootstrap`](Bootstrap.md) | Native-callable entry points used by the C++ dotnet_loader plugin. Every plugin assembly is loaded into its own colle... |

## Events & Enums

| Page | Content |
| --- | --- |
| [Events](events.md) | 55 event classes deriving from `Event` |
| [Enums](enums.md) | 16 public enums |

