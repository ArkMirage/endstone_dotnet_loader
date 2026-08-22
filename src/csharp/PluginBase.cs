using System.Reflection;

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
    public string[] Contributors { get; init; } = [];
    public string Website { get; init; } = "";
    public string Prefix { get; init; } = "";
    /// <summary>Default value for permissions registered by this plugin (Endstone default: Operator).</summary>
    public PermissionDefault DefaultPermission { get; init; } = PermissionDefault.Operator;
}

/// <summary>Base class for all .NET Endstone plugins.</summary>
public abstract class PluginBase : Plugin
{
    internal IntPtr GcHandle { get; set; }

    private readonly EventManager _events = new();
    private readonly CommandManager _commands = new();
    private readonly List<Permission> _permissions = new();
    private Logger? _logger;
    private Server? _server;
    private Scheduler? _scheduler;
    private ServiceManager? _serviceManager;

    public PluginBase() : base(new PluginDescription())
    {
        if (GetType().GetCustomAttribute<PluginAttribute>() is { } meta)
        {
            Description = PluginDescription.FromAttribute(meta);
        }
    }

    public Logger Logger => _logger ??= new Logger(this);

    public Server Server => _server ??= new Server(Bootstrap.ServerPtr);

    /// <summary>Plugin-scoped scheduler facade over the server's native scheduler.
    /// All tasks are cancelled automatically when the plugin is disabled.</summary>
    public Scheduler Scheduler => _scheduler ??= new Scheduler(Bootstrap.ServerPtr, NativeHandle);

    /// <summary>Plugin-scoped service manager facade over the server's native service
    /// manager. All registrations are dropped automatically when the plugin is disabled.</summary>
    public ServiceManager ServiceManager => _serviceManager ??= new ServiceManager(Bootstrap.ServerPtr, NativeHandle);

    /// <summary>Declares a plugin command (registered when the plugin is loaded). Fluent chain:
    /// <c>Command("hello").Description(...).Usage(...).Alias(...).Permission(...).Handler(handler)</c>.</summary>
    public CommandBuilder Command(string name) => _commands.Create(name);

    /// <summary>Creates a new permission owned by this plugin. Permissions created
    /// through this factory are registered automatically when the plugin is loaded
    /// (before OnLoad), mirroring the Python plugin's <c>permissions</c> declaration.
    /// Configure it with the fluent <c>With...</c> methods, e.g.
    /// <c>Permission("myplugin.kit").WithDefault(PermissionDefault.True)</c>.
    /// After registration the wrapper stays usable as a non-owning view of the
    /// registered permission; keep it in a plugin field to keep modifying it.
    /// Use <c>new Permission(...)</c> + <c>Register()</c> for permissions created
    /// at runtime.</summary>
    public Permission Permission(string name)
    {
        var permission = new Permission(name);
        _permissions.Add(permission);
        return permission;
    }

    /// <summary>Registers all permissions created through <c>Permission(name)</c>.
    /// Called by the loader before OnLoad; duplicate names are skipped with a
    /// warning and the wrapper keeps ownership (freed when collected).</summary>
    internal void RegisterPermissions()
    {
        foreach (var permission in _permissions)
        {
            if (!permission.Register())
            {
                Logger.Warning($"Permission '{permission.Name}' is already registered; skipping.");
            }
        }
    }

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

    /// <summary>Permissions declared by this plugin (created through the
    /// <c>Permission(name)</c> factory), reflecting the live list.</summary>
    internal override Permission[] GetPermissions() => _permissions.ToArray();

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
