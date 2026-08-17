namespace Endstone.Loader;

/// <summary>
/// Wraps the native endstone::PluginManager. Provides read-only access to the
/// plugin ecosystem: look up plugins by name, enumerate all loaded plugins and
/// query their enabled state.
/// </summary>
public sealed unsafe class PluginManager
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal PluginManager(IntPtr ptr) => _ptr = (void*)ptr;

    /// <summary>Gets the plugin with the given name (case-sensitive), or null
    /// if it is not loaded. .NET plugins resolve to their live PluginBase;
    /// native plugins resolve to a metadata snapshot.</summary>
    public Plugin? GetPlugin(string name)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            var plugin = T->PluginManagerGetPlugin(_ptr, p);
            return plugin == null ? null : Plugin.FromNative((IntPtr)plugin);
        }
    }

    /// <summary>Gets a list of all currently loaded plugins.</summary>
    public Plugin[] GetPlugins()
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var count = T->PluginManagerGetPlugins(_ptr, buffer, capacity);
        var plugins = new Plugin[count];
        for (var i = 0; i < count; i++)
        {
            plugins[i] = Plugin.FromNative((IntPtr)buffer[i]);
        }
        return plugins;
    }

    /// <summary>Checks whether the plugin with the given name is loaded and
    /// enabled (case-sensitive).</summary>
    public bool IsPluginEnabled(string name) => Bridge.CallBoolStr(T->PluginManagerIsPluginEnabled, _ptr, name);
}