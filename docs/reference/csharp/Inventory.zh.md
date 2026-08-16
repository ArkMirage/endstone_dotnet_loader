# Inventory

`class`

包装原生 endstone::Inventory 的托管对象,提供物品的读取、添加、移除与容量查询。

**命名空间** `Endstone.Loader`

**继承** `object`

**派生类** `PlayerInventory`

## 属性

### `IsEmpty` : `bool`

`{ get; }`

### `MaxStackSize` : `int`

`{ get; }`

### `Size` : `int`

`{ get; }`

## 方法

### `bool AddItem(ItemStack item)`

添加物品,不能完全装入时返回 false。

### `void Clear()`

### `bool Contains(ItemStack item)`

### `int First(string type)`

### `int FirstEmpty()`

### `ItemStack GetItem(int index)`

获取指定栏位的物品。

### `bool RemoveItem(ItemStack item)`

移除指定物品。

### `void SetItem(int index, ItemStack item)`

