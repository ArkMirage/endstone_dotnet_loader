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

    /// <summary>Gets the permissions declared by this plugin. Native plugins
    /// are queried through the bridge; .NET plugins override the source.</summary>
    public Permission[] Permissions => GetPermissions();

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