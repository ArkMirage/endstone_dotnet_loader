# ServiceManager

`sealed class`

Plugin-scoped facade over the server's native service manager (mirrors endstone::ServiceManager). Registering a provider associates it with this plugin; all registrations are dropped automatically when the plugin is disabled.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

### `Service Get(string name)`

Queries for a provider. Returns null if no provider has been registered for the service; otherwise the highest-priority provider is returned.

### `void Register(string name, Service provider, ServicePriority priority)`

Registers a provider for the given service name. The provider is kept alive until it is unregistered or the plugin is disabled.

### `void Unregister(Service provider)`

Unregisters a particular provider from every service it is registered for, releasing the pin on it.

### `void Unregister(string name, Service provider)`

Unregisters a particular provider for a particular service. The provider stays pinned while it remains registered under any other name.

### `void UnregisterAll()`

