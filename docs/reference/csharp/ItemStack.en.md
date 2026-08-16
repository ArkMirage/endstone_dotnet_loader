# ItemStack

`sealed class`

Wraps a native endstone::ItemStack read-only view. When created via ItemStack.Create, Dispose() frees the native stack. When wrapping a dropped item entity (isItemActor), RemoveFromWorld() removes it from the level.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Amount` : `int`

`{ get; }`

### `Damage` : `int`

`{ get; }`

### `Data` : `int`

`{ get; }`

### `DisplayName` : `string`

`{ get; }`

### `EnchantCount` : `int`

`{ get; }`

### `Enchantments` : `System.Collections.Generic.IReadOnlyList<ItemEnchantment>`

`{ get; }`

### `HasDamage` : `bool`

`{ get; }`

### `HasDisplayName` : `bool`

`{ get; }`

### `HasEnchants` : `bool`

`{ get; }`

### `HasLore` : `bool`

`{ get; }`

### `HasMapView` : `bool`

`{ get; }`

### `IsUnbreakable` : `bool`

`{ get; }`

### `LoreCount` : `int`

`{ get; }`

### `MapView` : `MapView`

`{ get; }`

### `MaxStackSize` : `int`

`{ get; }`

### `TranslationKey` : `string`

`{ get; }`

### `Type` : `string`

`{ get; }`

## Methods

### `bool AddEnchant(string id, int level, bool force)`

Adds the enchantment at the given level. When force is false the level must be within the enchantment's start..max range. Conflicts with existing enchantments are not checked; use HasConflictingEnchant first.

### `ItemStack Create(string type, int amount, int data)`

Creates a new item stack of the given type (e.g. "minecraft:diamond"). Caller owns it (Dispose it).

### `void Dispose()`

### `int GetEnchantLevel(int index)`

### `int GetEnchantLevel(string id)`

Gets the level of the given enchantment, or 0 when absent.

### `string GetEnchantName(int index)`

### `string GetLoreLine(int index)`

### `bool HasConflictingEnchant(string id)`

Whether any enchantment on the item conflicts with the given one.

### `bool HasEnchant(string id)`

Whether the item carries the given enchantment (id like "minecraft:sharpness").

### `bool RemoveEnchant(string id)`

Removes the enchantment if present. Returns true if it was removed.

### `void RemoveEnchants()`

### `void RemoveFromWorld()`

### `bool SetMapView(MapView map)`

Binds this map item to the given map view (only works on map item stacks).

