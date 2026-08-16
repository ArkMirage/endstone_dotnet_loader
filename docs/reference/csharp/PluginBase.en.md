# PluginBase

`abstract class`

Base class for all .NET Endstone plugins.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Properties

### `Logger` : `Logger`

`{ get; }`

### `Scheduler` : `Scheduler`

`{ get; }`

### `Server` : `Server`

`{ get; }`

### `ServiceManager` : `ServiceManager`

`{ get; }`

## Methods

### `CommandBuilder Command(string name)`

Declares a plugin command (registered when the plugin is loaded). Fluent chain: Command("hello").Description(...).Usage(...).Alias(...).Permission(...).Handler(handler).

### `void OnDisable()`

### `void OnEnable()`

### `void OnLoad()`

### `void RegisterEvent(System.Action<T> handler, EventPriority priority, bool ignoreCancelled)`

### `void RegisterEvent(string eventName, System.Action<Event> handler, EventPriority priority, bool ignoreCancelled)`

Registers an event handler for the given Endstone event name.

