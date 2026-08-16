# CommandBuilder

`sealed class`

Fluent builder that declares a plugin command.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

### `CommandBuilder Alias(params string[] aliases)`

### `CommandBuilder Description(string description)`

### `CommandBuilder Handler(System.Func<CommandSender, System.Collections.Generic.IReadOnlyList<string>, bool> handler)`

Sets the handler invoked when the command runs. Returns true on success.

### `CommandBuilder Permission(params string[] permissions)`

### `CommandBuilder Usage(params string[] usages)`

