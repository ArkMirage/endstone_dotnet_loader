# CommandSender

`sealed class`

Wraps a native endstone::CommandSender.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `IsPlayer` : `bool`

`{ get; }`

### `Name` : `string`

`{ get; }`

## Methods

### `Player AsPlayer()`

### `bool HasPermission(string permission)`

### `void SendErrorMessage(string message)`

### `void SendErrorMessage(string format, params object[] args)`

### `void SendMessage(string message)`

### `void SendMessage(string format, params object[] args)`

