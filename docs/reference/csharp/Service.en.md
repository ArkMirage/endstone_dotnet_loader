# Service

`abstract class`

Base class for services registered with the server's service manager (mirrors endstone::Service). A service is a marker interface: providers just derive from this class and implement their own methods. While a provider is registered it is pinned by the owning ServiceManager, so the plugin does not need to keep a field reference; keeping one is only useful if the plugin wants to unregister or reuse the provider later.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

### `void Finalize()`

