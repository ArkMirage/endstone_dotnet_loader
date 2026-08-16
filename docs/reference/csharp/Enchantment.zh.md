# Enchantment

`sealed class`

包装原生 endstone::Enchantment 的托管对象。通过 Enchantment.Get(键) 或静态常量(如 Sharpness)获取,并提供常用附魔的静态属性。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `AquaAffinity` : `Enchantment`

`static` `{ get; }`

### `BaneOfArthropods` : `Enchantment`

`static` `{ get; }`

### `BlastProtection` : `Enchantment`

`static` `{ get; }`

### `Breach` : `Enchantment`

`static` `{ get; }`

### `Channeling` : `Enchantment`

`static` `{ get; }`

### `CurseOfBinding` : `Enchantment`

`static` `{ get; }`

### `CurseOfVanishing` : `Enchantment`

`static` `{ get; }`

### `Density` : `Enchantment`

`static` `{ get; }`

### `DepthStrider` : `Enchantment`

`static` `{ get; }`

### `Efficiency` : `Enchantment`

`static` `{ get; }`

### `FeatherFalling` : `Enchantment`

`static` `{ get; }`

### `FireAspect` : `Enchantment`

`static` `{ get; }`

### `FireProtection` : `Enchantment`

`static` `{ get; }`

### `Flame` : `Enchantment`

`static` `{ get; }`

### `Fortune` : `Enchantment`

`static` `{ get; }`

### `FrostWalker` : `Enchantment`

`static` `{ get; }`

### `Id` : `string`

`{ get; }`

### `Impaling` : `Enchantment`

`static` `{ get; }`

### `Infinity` : `Enchantment`

`static` `{ get; }`

### `Key` : `string`

`{ get; }`

### `Knockback` : `Enchantment`

`static` `{ get; }`

### `Looting` : `Enchantment`

`static` `{ get; }`

### `Loyalty` : `Enchantment`

`static` `{ get; }`

### `LuckOfTheSea` : `Enchantment`

`static` `{ get; }`

### `Lunge` : `Enchantment`

`static` `{ get; }`

### `Lure` : `Enchantment`

`static` `{ get; }`

### `MaxLevel` : `int`

`{ get; }`

### `Mending` : `Enchantment`

`static` `{ get; }`

### `Multishot` : `Enchantment`

`static` `{ get; }`

### `Namespace` : `string`

`{ get; }`

### `Piercing` : `Enchantment`

`static` `{ get; }`

### `Power` : `Enchantment`

`static` `{ get; }`

### `ProjectileProtection` : `Enchantment`

`static` `{ get; }`

### `Protection` : `Enchantment`

`static` `{ get; }`

### `Punch` : `Enchantment`

`static` `{ get; }`

### `QuickCharge` : `Enchantment`

`static` `{ get; }`

### `Respiration` : `Enchantment`

`static` `{ get; }`

### `Riptide` : `Enchantment`

`static` `{ get; }`

### `Sharpness` : `Enchantment`

`static` `{ get; }`

### `SilkTouch` : `Enchantment`

`static` `{ get; }`

### `Smite` : `Enchantment`

`static` `{ get; }`

### `SoulSpeed` : `Enchantment`

`static` `{ get; }`

### `StartLevel` : `int`

`{ get; }`

### `SwiftSneak` : `Enchantment`

`static` `{ get; }`

### `Thorns` : `Enchantment`

`static` `{ get; }`

### `Unbreaking` : `Enchantment`

`static` `{ get; }`

### `WindBurst` : `Enchantment`

`static` `{ get; }`

## 方法

### `bool CanEnchantItem(ItemStack item)`

Checks whether this enchantment may be applied to the given item stack.

### `bool ConflictsWith(Enchantment other)`

Checks whether this enchantment conflicts with another one.

### `Enchantment Get(string id)`

按 ID(如 "minecraft:sharpness")获取附魔,未找到返回 null。

