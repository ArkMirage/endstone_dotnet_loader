namespace Endstone.Loader;

/// <summary>
/// Wraps a native endstone::CommandSender.
/// </summary>
public unsafe class CommandSender
{
    protected readonly void* _ptr;

    internal CommandSender(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Name => Bridge.Str(T->SenderGetName(_ptr));

    public bool IsPlayer => T->SenderAsPlayer(_ptr) != null;

    public void SendMessage(string message) => Bridge.Call1(T->SenderSendMessage, _ptr, message);

    public void SendMessage(string format, params object?[] args) => SendMessage(string.Format(format, args));

    public void SendErrorMessage(string message) => Bridge.Call1(T->SenderSendErrorMessage, _ptr, message);

    public void SendErrorMessage(string format, params object?[] args) => SendErrorMessage(string.Format(format, args));

    public bool HasPermission(string permission) => Bridge.CallBoolStr(T->SenderHasPermission, _ptr, permission);

    /// <summary>Checks whether this sender has the given permission object.</summary>
    public bool HasPermission(Permission permission) => T->SenderHasPermissionPerm(_ptr, (void*)permission.NativePtr);

    /// <summary>Gets the permission level of this sender.</summary>
    public PermissionLevel PermissionLevel => (PermissionLevel)T->SenderGetPermissionLevel(_ptr);

    /// <summary>Checks whether this sender contains an override for the given
    /// permission, by fully qualified name.</summary>
    public bool IsPermissionSet(string permission) => Bridge.CallBoolStr(T->SenderIsPermissionSet, _ptr, permission);

    /// <summary>Checks whether this sender contains an override for the given
    /// permission object.</summary>
    public bool IsPermissionSet(Permission permission) => T->SenderIsPermissionSetPerm(_ptr, (void*)permission.NativePtr);

    /// <summary>Adds a new permission attachment with a single permission by
    /// name and value. The attachment is owned by this sender and stays valid
    /// until removed (or this sender is destroyed).</summary>
    public PermissionAttachment? AddAttachment(PluginBase plugin, string name, bool value)
    {
        if (plugin.NativeHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("The plugin is not attached to the server.");
        }
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            var a = T->SenderAddAttachment(_ptr, (void*)plugin.NativeHandle, p, value);
            return a == null ? null : new PermissionAttachment((IntPtr)a);
        }
    }

    /// <summary>Adds a new empty permission attachment to this sender.</summary>
    public PermissionAttachment? AddAttachment(PluginBase plugin)
    {
        if (plugin.NativeHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("The plugin is not attached to the server.");
        }
        var a = T->SenderAddAttachmentEmpty(_ptr, (void*)plugin.NativeHandle);
        return a == null ? null : new PermissionAttachment((IntPtr)a);
    }

    /// <summary>Removes the given permission attachment from this sender.
    /// Returns true when removed successfully, false when it is not part of
    /// this sender.</summary>
    public bool RemoveAttachment(PermissionAttachment attachment)
        => T->SenderRemoveAttachment(_ptr, (void*)attachment.NativePtr);

    /// <summary>Recalculates the permissions for this sender, if the
    /// attachments have changed values.</summary>
    public void RecalculatePermissions() => T->SenderRecalculatePermissions(_ptr);

    /// <summary>Gets all the permissions currently in effect for this sender.</summary>
    public PermissionAttachmentInfo[] GetEffectivePermissions()
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var count = T->SenderGetEffectivePermissions(_ptr, buffer, capacity);
        var infos = new PermissionAttachmentInfo[count];
        for (var i = 0; i < count; i++)
        {
            infos[i] = new PermissionAttachmentInfo((IntPtr)buffer[i]);
        }
        return infos;
    }

    public Player? AsPlayer()
    {
        var p = T->SenderAsPlayer(_ptr);
        return p == null ? null : new Player((IntPtr)p);
    }

    /// <summary>Downcasts this sender to Actor, or null if it is not an actor.</summary>
    public Actor? AsActor()
    {
        var a = T->SenderAsActor(_ptr);
        return a == null ? null : new Actor((IntPtr)a);
    }

    /// <summary>Downcasts this sender to ConsoleCommandSender, or null if it is not the console.</summary>
    public ConsoleCommandSender? AsConsole()
    {
        var c = T->SenderAsConsole(_ptr);
        return c == null ? null : new ConsoleCommandSender((IntPtr)c);
    }
}

/// <summary>Wraps a native endstone::ConsoleCommandSender (the server console).</summary>
public sealed unsafe class ConsoleCommandSender : CommandSender
{
    internal ConsoleCommandSender(IntPtr ptr) : base(ptr) { }
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