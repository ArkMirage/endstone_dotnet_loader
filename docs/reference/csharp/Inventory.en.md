# Inventory

`class`

Wraps a native endstone::Inventory (server-owned, never deleted). Item snapshots returned by getters are transient copies — use them immediately.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

**Derived classes** `PlayerInventory`

## Properties

### `IsEmpty` : `bool`

`{ get; }`

### `MaxStackSize` : `int`

`{ get; }`

### `Size` : `int`

`{ get; }`

## Methods

### `bool AddItem(ItemStack item)`

### `void Clear()`

### `bool Contains(ItemStack item)`

### `int First(string type)`

### `int FirstEmpty()`

### `ItemStack GetItem(int index)`

### `bool RemoveItem(ItemStack item)`

### `void SetItem(int index, ItemStack item)`

