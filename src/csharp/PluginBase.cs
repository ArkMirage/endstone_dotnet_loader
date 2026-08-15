namespace Endstone.Loader;

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
}

/// <summary>Logger bound to a plugin's native Endstone logger.</summary>
public sealed class Logger
{
    private readonly PluginBase _plugin;

    internal Logger(PluginBase plugin) => _plugin = plugin;

    public void Log(LogLevel level, string message) => Bootstrap.Log(_plugin.NativeHandle, level, message);
    public void Log(LogLevel level, string format, params object?[] args) => Log(level, string.Format(format, args));

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message) => Log(LogLevel.Error, message);
    public void Critical(string message) => Log(LogLevel.Critical, message);

    public void Info(string format, params object?[] args) => Log(LogLevel.Info, string.Format(format, args));
    public void Warning(string format, params object?[] args) => Log(LogLevel.Warning, string.Format(format, args));
    public void Error(string format, params object?[] args) => Log(LogLevel.Error, string.Format(format, args));
    public void Debug(string format, params object?[] args) => Log(LogLevel.Debug, string.Format(format, args));
    public void Trace(string format, params object?[] args) => Log(LogLevel.Trace, string.Format(format, args));
}

/// <summary>
/// Attribute describing plugin metadata. The plugin name must contain only
/// lowercase letters, numbers and underscores (Endstone requirement).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PluginAttribute(string name, string version) : Attribute
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Description { get; init; } = "";
    public string[] Authors { get; init; } = [];
}

/// <summary>Base class for all .NET Endstone plugins.</summary>
public abstract class PluginBase
{
    internal IntPtr NativeHandle { get; set; }
    internal IntPtr GcHandle { get; set; }

    private readonly EventManager _events = new();
    private readonly CommandManager _commands = new();
    private Logger? _logger;
    private Server? _server;
    private Scheduler? _scheduler;

    public Logger Logger => _logger ??= new Logger(this);

    public Server Server => _server ??= new Server(Bootstrap.ServerPtr);

    /// <summary>Plugin-scoped scheduler facade over the server's native scheduler.
    /// All tasks are cancelled automatically when the plugin is disabled.</summary>
    public Scheduler Scheduler => _scheduler ??= new Scheduler(Bootstrap.ServerPtr, NativeHandle);

    /// <summary>Declares a plugin command (registered when the plugin is loaded). Fluent chain:
    /// <c>Command("hello").Description(...).Usage(...).Alias(...).Permission(...).Handler(handler)</c>.</summary>
    public CommandBuilder Command(string name) => _commands.Create(name);

    /// <summary>Registers an event handler for the given Endstone event name.</summary>
    public void RegisterEvent(string eventName, Action<Event> handler,
                              EventPriority priority = EventPriority.Normal, bool ignoreCancelled = false)
        => _events.Register(eventName, priority, ignoreCancelled, handler);

    /// <summary>Registers a strongly-typed event handler.</summary>
    public void RegisterEvent<T>(Action<T> handler, EventPriority priority = EventPriority.Normal,
                                 bool ignoreCancelled = false) where T : Event
        => _events.Register(typeof(T).Name, priority, ignoreCancelled, e => handler((T)e));

    internal bool DispatchCommand(string name, CommandSender sender, IReadOnlyList<string> args)
        => _commands.Dispatch(name, sender, args);

    internal IEnumerable<CommandDefinition> CommandDefinitions => _commands.Definitions;

    internal void SetPluginHandles(IntPtr gcHandle, IntPtr nativeHandle)
    {
        GcHandle = gcHandle;
        NativeHandle = nativeHandle;
        _events.SetPluginHandle(gcHandle);
    }

    public virtual void OnLoad() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}
