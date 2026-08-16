# ScheduledTask

`sealed class`

排队到原生 endstone 调度器的任务句柄。同步任务在服务器线程执行,异步任务在工作线程执行。

**命名空间** `Endstone.Loader`

**继承** `object`

## 构造函数

- `ScheduledTask(Scheduler scheduler, uint taskId, bool isSync)` Handle for a task queued on the native endstone scheduler. Sync tasks run on the server thread, async tasks on a scheduler worker thread.

## 属性

### `IsQueued` : `bool`

`{ get; }`

任务是否仍在队列中。

### `IsRunning` : `bool`

`{ get; }`

任务是否正在运行。

### `IsSync` : `bool`

`{ get; }`

任务是否为同步任务。

### `TaskId` : `uint`

`{ get; }`

## 方法

### `void Cancel()`

