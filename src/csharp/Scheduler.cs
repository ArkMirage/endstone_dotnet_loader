using System.Runtime.InteropServices;

namespace Endstone.Loader;

/// <summary>
/// Handle for a task queued on the native endstone scheduler. Sync tasks run on
/// the server thread, async tasks on a scheduler worker thread.
/// </summary>
public sealed class ScheduledTask(Scheduler scheduler, uint taskId, bool isSync)
{
    public uint TaskId { get; } = taskId;
    public bool IsSync { get; } = isSync;

    public bool IsQueued => scheduler.IsQueued(TaskId);
    public bool IsRunning => scheduler.IsRunning(TaskId);
    public void Cancel() => scheduler.CancelTask(TaskId);
}

/// <summary>
/// Plugin-scoped facade over the native endstone Scheduler. Tasks are owned by
/// the plugin, so all pending tasks are cancelled when the plugin is disabled.
/// </summary>
public sealed unsafe class Scheduler
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _scheduler;
    private readonly void* _plugin;
    private readonly object _lock = new();
    private readonly Dictionary<ulong, uint> _tasks = new();

    internal Scheduler(IntPtr serverPtr, IntPtr pluginPtr)
    {
        _scheduler = T->ServerGetScheduler((void*)serverPtr);
        _plugin = (void*)pluginPtr;
    }

    private static long _nextManagedId;

    public ScheduledTask RunTask(Action callback) => Schedule(0, 0, 0, callback);
    public ScheduledTask RunTaskLater(Action callback, ulong delayTicks) => Schedule(1, delayTicks, 0, callback);
    public ScheduledTask RunTaskTimer(Action callback, ulong delayTicks, ulong periodTicks)
        => Schedule(2, delayTicks, periodTicks, callback);
    public ScheduledTask RunTaskAsync(Action callback) => Schedule(3, 0, 0, callback);
    public ScheduledTask RunTaskLaterAsync(Action callback, ulong delayTicks)
        => Schedule(4, delayTicks, 0, callback);
    public ScheduledTask RunTaskTimerAsync(Action callback, ulong delayTicks, ulong periodTicks)
        => Schedule(5, delayTicks, periodTicks, callback);

    private ScheduledTask Schedule(int mode, ulong delay, ulong period, Action callback)
    {
        var managedId = (ulong)Interlocked.Increment(ref _nextManagedId);
        uint nativeId;
        lock (_lock)
        {
            nativeId = T->SchedulerRunTask(_scheduler, _plugin, mode, delay, period, managedId);
            if (nativeId == 0)
            {
                return new ScheduledTask(this, 0, !IsAsyncMode(mode));
            }
            _tasks[managedId] = nativeId;
        }
        SchedulerRegistry.Register(managedId, period == 0, callback);
        return new ScheduledTask(this, nativeId, !IsAsyncMode(mode));
    }

    private static bool IsAsyncMode(int mode) => mode >= 3;

    public void CancelTask(uint nativeId)
    {
        lock (_lock)
        {
            T->SchedulerCancelTask(_scheduler, nativeId);
            foreach (var (managedId, id) in _tasks)
            {
                if (id == nativeId)
                {
                    _tasks.Remove(managedId);
                    break;
                }
            }
        }
    }

    public bool IsQueued(uint nativeId) => T->SchedulerIsQueued(_scheduler, nativeId);
    public bool IsRunning(uint nativeId) => T->SchedulerIsRunning(_scheduler, nativeId);

    /// <summary>All tasks currently queued or running (native view).</summary>
    public List<ScheduledTask> GetPendingTasks()
    {
        var buffer = new void*[64];
        int count;
        fixed (void** p = buffer)
        {
            count = T->SchedulerGetPendingTasks(_scheduler, p, buffer.Length);
        }
        var result = new List<ScheduledTask>(Math.Min(count, buffer.Length));
        for (var i = 0; i < Math.Min(count, buffer.Length); i++)
        {
            var taskPtr = buffer[i];
            bool isSync = T->TaskIsSync(taskPtr);
            var nativeId = T->TaskGetId(taskPtr);
            ScheduledTask existing = new(this, nativeId, isSync);
            result.Add(existing);
        }
        return result;
    }

    /// <summary>Cancels all tasks owned by this plugin (called on plugin disable).</summary>
    public void CancelAll()
    {
        lock (_lock)
        {
            T->SchedulerCancelTasks(_scheduler, _plugin);
            SchedulerRegistry.UnregisterAll(_tasks.Keys);
            _tasks.Clear();
        }
    }

    internal static void Fire(ulong managedId)
    {
        if (SchedulerRegistry.Take(managedId, out var oneShot, out var callback))
        {
            callback();
        }
    }
}

/// <summary>Registry correlating native fire callbacks with managed delegates.</summary>
internal static class SchedulerRegistry
{
    private sealed record Entry(bool OneShot, Action Callback);

    private static readonly object Lock = new();
    private static readonly Dictionary<ulong, Entry> Entries = new();

    internal static void Register(ulong managedId, bool oneShot, Action callback)
    {
        lock (Lock)
        {
            Entries[managedId] = new Entry(oneShot, callback);
        }
    }

    /// <summary>Removes the entry (fires callbacks must be re-armed first).</summary>
    internal static void UnregisterAll(IEnumerable<ulong> managedIds)
    {
        lock (Lock)
        {
            foreach (var id in managedIds)
            {
                Entries.Remove(id);
            }
        }
    }

    internal static bool Take(ulong managedId, out bool oneShot, out Action callback)
    {
        lock (Lock)
        {
            if (Entries.TryGetValue(managedId, out var entry))
            {
                oneShot = entry.OneShot;
                callback = entry.Callback;
                if (entry.OneShot)
                {
                    Entries.Remove(managedId);
                }
                return true;
            }
            oneShot = true;
            callback = null!;
            return false;
        }
    }
}