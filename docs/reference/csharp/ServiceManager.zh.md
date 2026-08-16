# ServiceManager

`sealed class`

插件作用域的服务管理器外壳(对应 endstone::ServiceManager)。注册的提供者关联到本插件,插件停用时自动注销。

**命名空间** `Endstone.Loader`

**继承** `object`

## 方法

### `Service Get(string name)`

查询提供者,未注册时返回 null,多个提供者时返回优先级最高者。

### `void Register(string name, Service provider, ServicePriority priority)`

Registers a provider for the given service name. The provider is kept alive until it is unregistered or the plugin is disabled.

### `void Unregister(Service provider)`

从所有服务中注销该提供者。

### `void Unregister(string name, Service provider)`

Unregisters a particular provider for a particular service. The provider stays pinned while it remains registered under any other name.

### `void UnregisterAll()`

