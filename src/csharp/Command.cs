namespace Endstone.Loader;

/// <summary>Wraps a native endstone::CommandSender.</summary>
public sealed unsafe class CommandSender
{
    private readonly void* _ptr;

    internal CommandSender(IntPtr ptr) => _ptr = (void*)ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Name => Bridge.Str(T->SenderGetName(_ptr));

    public bool IsPlayer => T->SenderAsPlayer(_ptr) != null;

    public void SendMessage(string message) => Bridge.Call1(T->SenderSendMessage, _ptr, message);

    public void SendMessage(string format, params object?[] args) => SendMessage(string.Format(format, args));

    public void SendErrorMessage(string message) => Bridge.Call1(T->SenderSendErrorMessage, _ptr, message);

    public void SendErrorMessage(string format, params object?[] args) => SendErrorMessage(string.Format(format, args));

    public bool HasPermission(string permission) => Bridge.CallBoolStr(T->SenderHasPermission, _ptr, permission);

    public Player? AsPlayer()
    {
        var p = T->SenderAsPlayer(_ptr);
        return p == null ? null : new Player((IntPtr)p);
    }
}

/// <summary>Fluent builder that declares a plugin command.</summary>
public sealed class CommandBuilder
{
    private readonly string _name;
    private string _description = "";
    private readonly List<string> _usages = new();
    private readonly List<string> _aliases = new();
    private readonly List<string> _permissions = new();
    private Func<CommandSender, IReadOnlyList<string>, bool>? _handler;

    internal CommandBuilder(string name) => _name = name;

    public CommandBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public CommandBuilder Usage(params string[] usages)
    {
        _usages.AddRange(usages);
        return this;
    }

    public CommandBuilder Alias(params string[] aliases)
    {
        _aliases.AddRange(aliases);
        return this;
    }

    public CommandBuilder Permission(params string[] permissions)
    {
        _permissions.AddRange(permissions);
        return this;
    }

    /// <summary>Sets the handler invoked when the command runs. Returns true on success.</summary>
    public CommandBuilder Handler(Func<CommandSender, IReadOnlyList<string>, bool> handler)
    {
        _handler = handler;
        return this;
    }

    internal CommandDefinition CommandDefinition => new(_name, _description, _usages.ToArray(), _aliases.ToArray(), _permissions.ToArray());

    internal bool TryExecute(CommandSender sender, IReadOnlyList<string> args)
    {
        return _handler != null && _handler(sender, args);
    }
}

/// <summary>Tracks the commands declared by a managed plugin.</summary>
internal sealed class CommandManager
{
    private readonly Dictionary<string, CommandBuilder> _commands = new(StringComparer.OrdinalIgnoreCase);

    internal CommandBuilder Create(string name)
    {
        var builder = new CommandBuilder(name);
        _commands[name.ToLowerInvariant()] = builder;
        return builder;
    }

    internal IEnumerable<CommandDefinition> Definitions => _commands.Values.Select(c => c.CommandDefinition);

    internal bool Dispatch(string name, CommandSender sender, IReadOnlyList<string> args)
    {
        return _commands.TryGetValue(name, out var builder) && builder.TryExecute(sender, args);
    }
}

internal record CommandDefinition(string Name, string Description, string[] Usages, string[] Aliases, string[] Permissions);