# CommandSender

`sealed class`

命令发送者的抽象,既可以是玩家也可以是控制台。通过 AsPlayer() 可判断并转型。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `IsPlayer` : `bool`

`{ get; }`

### `Name` : `string`

`{ get; }`

发送者的名称。

## 方法

### `Player AsPlayer()`

### `bool HasPermission(string permission)`

检查发送者是否拥有指定权限。

### `void SendErrorMessage(string message)`

### `void SendErrorMessage(string format, params object[] args)`

### `void SendMessage(string message)`

向发送者发送消息。

### `void SendMessage(string format, params object[] args)`

