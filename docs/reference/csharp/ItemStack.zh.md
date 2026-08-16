# ItemStack

`sealed class`

包装原生 endstone::ItemStack 的托管对象。使用 ItemStack.Create("minecraft:diamond", 数量) 创建,可读写附魔、显示名与 lore。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

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

物品上的全部附魔列表。

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

## 方法

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

是否拥有指定附魔。

### `bool RemoveEnchant(string id)`

移除指定附魔。

### `void RemoveEnchants()`

### `void RemoveFromWorld()`

### `bool SetMapView(MapView map)`

Binds this map item to the given map view (only works on map item stacks).

