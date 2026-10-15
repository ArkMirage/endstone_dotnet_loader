using System.Runtime.InteropServices;
using System.Text.Json;

namespace Endstone.Loader;

/// <summary>Represents the order in which a plugin should be initialized and enabled.</summary>
public enum PluginLoadOrder
{
    Startup = 0,
    PostWorld = 1,
}

/// <summary>
/// Immutable snapshot of a plugin's metadata (mirrors endstone::PluginDescription).
/// Transferred across the bridge as camelCase JSON for native plugins; .NET
/// plugins build it from their [Plugin] attribute at load time.
/// </summary>
public sealed class PluginDescription
{
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string FullName { get; init; } = "";
    public string ApiVersion { get; init; } = "";
    public string Description { get; init; } = "";
    public PluginLoadOrder Load { get; init; }
    public string[] Authors { get; init; } = [];
    public string[] Contributors { get; init; } = [];
    public string Website { get; init; } = "";
    public string Prefix { get; init; } = "";
    public string[] Provides { get; init; } = [];
    public PermissionDefault DefaultPermission { get; init; }

    /// <summary>Builds the description from a [Plugin] attribute. Called by
    /// the loader when the plugin instance is created.</summary>
    internal static PluginDescription FromAttribute(PluginAttribute meta) => new()
    {
        Name = meta.Name,
        Version = meta.Version,
        FullName = $"{meta.Name} v{meta.Version}",
        Description = meta.Description,
        Load = PluginLoadOrder.PostWorld,
        Authors = meta.Authors,
        Contributors = meta.Contributors,
        Website = meta.Website,
        Prefix = meta.Prefix,
        DefaultPermission = meta.DefaultPermission,
    };
}

/// <summary>
/// Read-only view of a plugin's metadata, returned by PluginManager for every
/// plugin on the server (both .NET and native). Carries no operational API:
/// it exists so plugins can programmatically inspect the plugin ecosystem.
/// </summary>
public unsafe class Plugin
{
    private static Bridge.Table* T => Bridge.Raw;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // Native Plugin* used to fetch permissions; kept internal (never exposed
    // publicly) and zero for .NET plugins, which override the permission source.
    internal IntPtr NativeHandle { get; set; }

    internal Plugin(IntPtr nativePtr)
    {
        NativeHandle = nativePtr;
        var json = Bridge.Str(T->PluginGetDescriptionJson((void*)nativePtr));
        Description = JsonSerializer.Deserialize<PluginDescription>(json, JsonOptions) ?? new PluginDescription();
    }

    internal Plugin(PluginDescription description) => Description = description;

    /// <summary>Resolves a native Plugin* to the managed view: the live
    /// PluginBase for .NET plugins, otherwise a fresh metadata snapshot.</summary>
    internal static Plugin FromNative(IntPtr nativePtr)
        => Bootstrap.FindPlugin(nativePtr) ?? new Plugin(nativePtr);

    public PluginDescription Description { get; internal set; }

    public string Name => Description.Name;
    public string Version => Description.Version;
    public string FullName => Description.FullName;
    public string ApiVersion => Description.ApiVersion;
    public string DescriptionText => Description.Description;
    public PluginLoadOrder Load => Description.Load;
    public string[] Authors => Description.Authors;
    public string[] Contributors => Description.Contributors;
    public string Website => Description.Website;
    public string Prefix => Description.Prefix;
    public string[] Provides => Description.Provides;
    public PermissionDefault DefaultPermission => Description.DefaultPermission;

    /// <summary>Gets the folder that holds this plugin's data files. The path
    /// is resolved natively (UTF-8), and the folder may not exist yet. Returns
    /// an empty string when the plugin has no native handle.</summary>
    public string DataFolder =>
        NativeHandle == IntPtr.Zero ? "" : Bridge.Str(T->PluginGetDataFolder((void*)NativeHandle));

    /// <summary>Gets the permissions declared by this plugin. Native plugins
    /// are queried through the bridge; .NET plugins override the source.</summary>
    public Permission[] Permissions => GetPermissions();

    // The permission registry is owned by the server's native PluginManager;
    // these members reach it through this plugin's native handle (the C++
    // equivalent of Plugin::getServer().getPluginManager()). A metadata-only
    // snapshot of a native plugin has no handle and cannot be used.
    private void* PermissionHandle
    {
        get
        {
            if (NativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("This plugin has no native handle.");
            }
            return (void*)NativeHandle;
        }
    }

    /// <summary>Looks up a registered permission by its fully qualified name
    /// (case-insensitive). Returns null when no such permission is registered.
    /// The returned wrapper is a non-owning view of the plugin-manager-owned
    /// permission.</summary>
    public Permission? GetPermission(string name)
    {
        var p = Bridge.CallPtrStr(T->PluginFindPermission, PermissionHandle, name);
        return p == null ? null : new Permission((IntPtr)p);
    }

    /// <summary>Removes the named permission's registration from the server's
    /// plugin manager. Returns whether the permission was registered. The
    /// native object is destroyed, so any wrapper for it becomes invalid.
    /// Permissibles that already have the permission keep it.</summary>
    public bool RemovePermission(string name)
        => Bridge.CallBoolStr(T->PluginRemovePermission, PermissionHandle, name);

    /// <summary>Gets the permissions whose default value applies to the given
    /// permission level. The returned wrappers are non-owning views of the
    /// plugin-manager-owned permissions.</summary>
    public Permission[] GetDefaultPermissions(PermissionLevel level)
        => Wrap(Bridge.CallPtrArrayInt(T->PluginGetDefaultPermissions, PermissionHandle, (int)level));

    /// <summary>Recalculates the defaults of the given permission. Has no
    /// effect when the permission is not registered with the plugin manager.</summary>
    public void RecalculatePermissionDefaults(Permission permission)
        => T->PluginRecalculatePermissionDefaults(PermissionHandle, (void*)permission.NativePtr);

    /// <summary>Subscribes the given permissible for changes to the named
    /// permission: any change makes it recalculate its permissions.</summary>
    public void SubscribeToPermission(string name, CommandSender permissible)
        => CallPermissible(T->PluginSubscribeToPermission, name, permissible);

    /// <summary>Unsubscribes the given permissible from the named permission.</summary>
    public void UnsubscribeFromPermission(string name, CommandSender permissible)
        => CallPermissible(T->PluginUnsubscribeFromPermission, name, permissible);

    /// <summary>Gets all permissibles subscribed to the named permission.</summary>
    public CommandSender[] GetPermissionSubscriptions(string name)
        => WrapSenders(Bridge.CallPtrArrayStr(T->PluginGetPermissionSubscriptions, PermissionHandle, name));

    /// <summary>Subscribes the given permissible for changes to the defaults of
    /// the given permission level.</summary>
    public void SubscribeToDefaultPerms(PermissionLevel level, CommandSender permissible)
        => T->PluginSubscribeToDefaultPerms(PermissionHandle, (int)level, (void*)permissible.NativePtr);

    /// <summary>Unsubscribes the given permissible from the defaults of the
    /// given permission level.</summary>
    public void UnsubscribeFromDefaultPerms(PermissionLevel level, CommandSender permissible)
        => T->PluginUnsubscribeFromDefaultPerms(PermissionHandle, (int)level, (void*)permissible.NativePtr);

    /// <summary>Gets all permissibles subscribed to the defaults of the given
    /// permission level.</summary>
    public CommandSender[] GetDefaultPermSubscriptions(PermissionLevel level)
        => WrapSenders(Bridge.CallPtrArrayInt(T->PluginGetDefaultPermSubscriptions, PermissionHandle, (int)level));

    private void CallPermissible(delegate* unmanaged[Cdecl]<void*, byte*, void*, void> fn,
                                 string name, CommandSender permissible)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            fn(PermissionHandle, p, (void*)permissible.NativePtr);
        }
    }

    private static Permission[] Wrap(void*[] pointers)
    {
        var permissions = new Permission[pointers.Length];
        for (var i = 0; i < pointers.Length; i++)
        {
            permissions[i] = new Permission((IntPtr)pointers[i]);
        }
        return permissions;
    }

    private static CommandSender[] WrapSenders(void*[] pointers)
    {
        var senders = new CommandSender[pointers.Length];
        for (var i = 0; i < pointers.Length; i++)
        {
            senders[i] = new CommandSender((IntPtr)pointers[i]);
        }
        return senders;
    }

    internal virtual Permission[] GetPermissions()
    {
        if (NativeHandle == IntPtr.Zero)
        {
            return [];
        }
        var count = T->PluginGetPermissionCount((void*)NativeHandle);
        var permissions = new Permission[count];
        for (var i = 0; i < count; i++)
        {
            permissions[i] = new Permission((IntPtr)T->PluginGetPermission((void*)NativeHandle, i));
        }
        return permissions;
    }
}