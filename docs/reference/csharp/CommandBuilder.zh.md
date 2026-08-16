# CommandBuilder

`sealed class`

命令的流式构建器:配置描述、用法、别名、权限,并绑定命令处理函数。

**命名空间** `Endstone.Loader`

**继承** `object`

## 方法

### `CommandBuilder Alias(params string[] aliases)`

### `CommandBuilder Description(string description)`

### `CommandBuilder Handler(System.Func<CommandSender, System.Collections.Generic.IReadOnlyList<string>, bool> handler)`

Sets the handler invoked when the command runs. Returns true on success.

### `CommandBuilder Permission(params string[] permissions)`

### `CommandBuilder Usage(params string[] usages)`

