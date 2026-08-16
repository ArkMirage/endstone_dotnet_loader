# PluginBase

`abstract class`

所有 .NET 插件的基类。继承并在主类上添加 [Plugin] 特性;重写 OnLoad/OnEnable/OnDisable 生命周期方法,通过 Command()/RegisterEvent()/Scheduler/ServiceManager 使用服务器功能。

**命名空间** `Endstone.Loader`

**继承** `object`

## 属性

### `Logger` : `Logger`

`{ get; }`

插件的日志记录器。

### `Scheduler` : `Scheduler`

`{ get; }`

插件作用域的调度器。

### `Server` : `Server`

`{ get; }`

服务器实例的访问器。

### `ServiceManager` : `ServiceManager`

`{ get; }`

插件作用域的服务管理器。

## 方法

### `CommandBuilder Command(string name)`

注册命令并返回流式构建器。

### `void OnDisable()`

### `void OnEnable()`

### `void OnLoad()`

### `void RegisterEvent(System.Action<T> handler, EventPriority priority, bool ignoreCancelled)`

### `void RegisterEvent(string eventName, System.Action<Event> handler, EventPriority priority, bool ignoreCancelled)`

Registers an event handler for the given Endstone event name.

