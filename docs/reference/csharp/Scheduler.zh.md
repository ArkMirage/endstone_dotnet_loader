# Scheduler

`sealed class`

插件作用域的调度器外壳,封装原生 endstone Scheduler。任务归属于插件,插件停用时自动全部取消。

**命名空间** `Endstone.Loader`

**继承** `object`

## 方法

### `void CancelAll()`

### `void CancelTask(uint nativeId)`

### `System.Collections.Generic.List<ScheduledTask> GetPendingTasks()`

### `bool IsQueued(uint nativeId)`

### `bool IsRunning(uint nativeId)`

### `ScheduledTask RunTask(System.Action callback)`

### `ScheduledTask RunTaskAsync(System.Action callback)`

### `ScheduledTask RunTaskLater(System.Action callback, ulong delayTicks)`

### `ScheduledTask RunTaskLaterAsync(System.Action callback, ulong delayTicks)`

### `ScheduledTask RunTaskTimer(System.Action callback, ulong delayTicks, ulong periodTicks)`

### `ScheduledTask RunTaskTimerAsync(System.Action callback, ulong delayTicks, ulong periodTicks)`

