# ScheduledTask

`sealed class`

Handle for a task queued on the native endstone scheduler. Sync tasks run on the server thread, async tasks on a scheduler worker thread.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Constructors

- `ScheduledTask(Scheduler scheduler, uint taskId, bool isSync)` Handle for a task queued on the native endstone scheduler. Sync tasks run on the server thread, async tasks on a scheduler worker thread.

## Properties

### `IsQueued` : `bool`

`{ get; }`

### `IsRunning` : `bool`

`{ get; }`

### `IsSync` : `bool`

`{ get; }`

### `TaskId` : `uint`

`{ get; }`

## Methods

### `void Cancel()`

