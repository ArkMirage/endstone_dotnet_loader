# Scheduler

`sealed class`

Plugin-scoped facade over the native endstone Scheduler. Tasks are owned by the plugin, so all pending tasks are cancelled when the plugin is disabled.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

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

