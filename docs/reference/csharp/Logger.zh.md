# Logger

`sealed class`

插件日志记录器,输出到服务器的插件日志。Trace 级别默认被过滤,Info 及以上可见。

**命名空间** `Endstone.Loader`

**继承** `object`

## 方法

### `void Critical(string message)`

### `void Debug(string message)`

### `void Debug(string format, params object[] args)`

### `void Error(string message)`

### `void Error(string format, params object[] args)`

### `void Info(string message)`

### `void Info(string format, params object[] args)`

### `void Log(LogLevel level, string message)`

### `void Log(LogLevel level, string format, params object[] args)`

### `void Trace(string message)`

### `void Trace(string format, params object[] args)`

### `void Warning(string message)`

### `void Warning(string format, params object[] args)`

