# Enums

Public enums used by the Endstone.Loader API.

| Enum | Purpose |
| --- | --- |
| [`BarColor`](#barcolor) |  |
| [`BarFlag`](#barflag) |  |
| [`BarStyle`](#barstyle) |  |
| [`BlockFace`](#blockface) |  |
| [`DimensionType`](#dimensiontype) |  |
| [`EquipmentSlot`](#equipmentslot) |  |
| [`EventPriority`](#eventpriority) |  |
| [`FormControlKind`](#formcontrolkind) |  |
| [`FormKind`](#formkind) |  |
| [`GameMode`](#gamemode) |  |
| [`InteractAction`](#interactaction) |  |
| [`LoadType`](#loadtype) |  |
| [`LogLevel`](#loglevel) |  |
| [`MapCursorType`](#mapcursortype) |  |
| [`MapScale`](#mapscale) |  |
| [`ServicePriority`](#servicepriority) | Represents the priority of a service provider. Higher-priority providers are returned by ServiceManager.Get first. |

## `BarColor`

| Value | Name |
| --- | --- |
| `0` | `Pink` |
| `1` | `Blue` |
| `2` | `Red` |
| `3` | `Green` |
| `4` | `Yellow` |
| `5` | `Purple` |
| `6` | `RebeccaPurple` |
| `7` | `White` |

---

## `BarFlag`

| Value | Name |
| --- | --- |
| `0` | `None` |
| `1` | `DarkenSky` |
| `2` | `CreateFog` |

---

## `BarStyle`

| Value | Name |
| --- | --- |
| `0` | `Solid` |
| `1` | `Segmented6` |
| `2` | `Segmented10` |
| `3` | `Segmented12` |
| `4` | `Segmented20` |

---

## `BlockFace`

| Value | Name |
| --- | --- |
| `0` | `Down` |
| `1` | `Up` |
| `2` | `North` |
| `3` | `South` |
| `4` | `West` |
| `5` | `East` |

---

## `DimensionType`

| Value | Name |
| --- | --- |
| `0` | `Overworld` |
| `1` | `Nether` |
| `2` | `TheEnd` |
| `999` | `Custom` |

---

## `EquipmentSlot`

| Value | Name |
| --- | --- |
| `0` | `Hand` |
| `1` | `OffHand` |
| `2` | `Feet` |
| `3` | `Legs` |
| `4` | `Chest` |
| `5` | `Head` |
| `6` | `Body` |

---

## `EventPriority`

| Value | Name |
| --- | --- |
| `0` | `Lowest` |
| `1` | `Low` |
| `2` | `Normal` |
| `3` | `High` |
| `4` | `Highest` |
| `5` | `Monitor` |

---

## `FormControlKind`

| Value | Name |
| --- | --- |
| `0` | `Label` |
| `1` | `Header` |
| `2` | `Divider` |
| `3` | `Dropdown` |
| `4` | `Slider` |
| `5` | `StepSlider` |
| `6` | `TextInput` |
| `7` | `Toggle` |

---

## `FormKind`

| Value | Name |
| --- | --- |
| `0` | `MessageForm` |
| `1` | `ActionForm` |
| `2` | `ModalForm` |

---

## `GameMode`

| Value | Name |
| --- | --- |
| `0` | `Survival` |
| `1` | `Creative` |
| `2` | `Adventure` |
| `3` | `Spectator` |

---

## `InteractAction`

| Value | Name |
| --- | --- |
| `0` | `LeftClickBlock` |
| `1` | `RightClickBlock` |
| `2` | `LeftClickAir` |
| `3` | `RightClickAir` |
| `4` | `Physical` |

---

## `LoadType`

| Value | Name |
| --- | --- |
| `0` | `Startup` |
| `1` | `Reload` |

---

## `LogLevel`

| Value | Name |
| --- | --- |
| `0` | `Trace` |
| `1` | `Debug` |
| `2` | `Info` |
| `3` | `Warning` |
| `4` | `Error` |
| `5` | `Critical` |

---

## `MapCursorType`

| Value | Name |
| --- | --- |
| `0` | `Player` |
| `1` | `Frame` |
| `2` | `RedMarker` |
| `3` | `BlueMarker` |
| `4` | `TargetX` |
| `5` | `TargetPoint` |
| `6` | `PlayerOffMap` |
| `7` | `SignMarker` |
| `8` | `PinkMarker` |
| `9` | `OrangeMarker` |
| `10` | `YellowMarker` |
| `11` | `CyanMarker` |
| `12` | `GreenPoint` |
| `13` | `PlayerOffLimits` |
| `14` | `Mansion` |
| `15` | `Monument` |
| `17` | `VillageDesert` |
| `18` | `VillagePlains` |
| `19` | `VillageSavanna` |
| `20` | `VillageSnowy` |
| `21` | `VillageTaiga` |
| `22` | `JungleTemple` |
| `23` | `SwampHut` |
| `24` | `TrialChambers` |

---

## `MapScale`

| Value | Name |
| --- | --- |
| `0` | `Closest` |
| `1` | `Close` |
| `2` | `Normal` |
| `3` | `Far` |
| `4` | `Farthest` |

---

## `ServicePriority`

Represents the priority of a service provider. Higher-priority providers are returned by ServiceManager.Get first.

| Value | Name |
| --- | --- |
| `0` | `Lowest` |
| `1` | `Low` |
| `2` | `Normal` |
| `3` | `High` |
| `4` | `Highest` |

---

