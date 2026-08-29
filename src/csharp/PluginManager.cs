using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Endstone.Loader;

/// <summary>
/// Wraps the native endstone::PluginManager. Provides read-only access to the
/// plugin ecosystem: look up plugins by name, enumerate all loaded plugins and
/// query their enabled state. Also lets a plugin register a custom
/// <see cref="PluginLoader"/>.
/// </summary>
public sealed unsafe class PluginManager
{
    private static Bridge.Table* T => Bridge.Raw;

    // Kept alive for the server lifetime: a registered loader cannot be
    // unregistered, so its managed handle must survive until shutdown.
    private static readonly List<GCHandle> RegisteredLoaders = [];

    private readonly void* _ptr;

    internal PluginManager(IntPtr ptr) => _ptr = (void*)ptr;

    /// <summary>
    /// Registers a custom <see cref="PluginLoader"/>. After registration the
    /// given <paramref name="directory"/> is scanned and every file matching the
    /// loader's <see cref="PluginLoader.FileFilters"/> is loaded as a plugin.
    /// The loader stays registered for the server lifetime (there is no
    /// unregister); point <paramref name="directory"/> at a folder dedicated to
    /// the loader's files to avoid clashing with other loaders.
    /// </summary>
    public void RegisterLoader(PluginLoader loader, string directory)
    {
        var gc = GCHandle.Alloc(loader);
        RegisteredLoaders.Add(gc);
        var dirBuf = Bridge.ToUtf8(directory);
        fixed (byte* p = dirBuf)
        {
            Bridge.CallRegisterLoader(_ptr, (void*)GCHandle.ToIntPtr(gc), p);
        }
    }

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

    /// <summary>Enables the given plugin. Enabling a plugin that is already
    /// enabled has no effect. The plugin must have a native handle (i.e. be a
    /// loaded plugin); metadata-only snapshots cannot be enabled.</summary>
    public void EnablePlugin(Plugin plugin)
    {
        if (plugin.NativeHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Cannot enable a plugin without a native handle.", nameof(plugin));
        }
        T->PluginManagerEnablePlugin(_ptr, (void*)plugin.NativeHandle);
    }

    /// <summary>Disables the given plugin. Disabling a plugin that is not
    /// enabled has no effect. The plugin must have a native handle (i.e. be a
    /// loaded plugin); metadata-only snapshots cannot be disabled.</summary>
    public void DisablePlugin(Plugin plugin)
    {
        if (plugin.NativeHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Cannot disable a plugin without a native handle.", nameof(plugin));
        }
        T->PluginManagerDisablePlugin(_ptr, (void*)plugin.NativeHandle);
    }

    /// <summary>Loads the plugin contained in the specified file. Returns the
    /// loaded plugin, or null if the file was invalid.</summary>
    public Plugin? LoadPlugin(string file)
    {
        var buf = Bridge.ToUtf8(file);
        fixed (byte* p = buf)
        {
            var plugin = T->PluginManagerLoadPlugin(_ptr, p);
            return plugin == null ? null : Plugin.FromNative((IntPtr)plugin);
        }
    }

    /// <summary>Loads all plugins contained within the specified directory.
    /// Returns the list of plugins that were loaded.</summary>
    public Plugin[] LoadPlugins(string directory)
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var buf = Bridge.ToUtf8(directory);
        fixed (byte* p = buf)
        {
            var count = T->PluginManagerLoadPluginsDir(_ptr, p, buffer, capacity);
            var plugins = new Plugin[count];
            for (var i = 0; i < count; i++)
            {
                plugins[i] = Plugin.FromNative((IntPtr)buffer[i]);
            }
            return plugins;
        }
    }

    /// <summary>Loads the plugins contained within the specified files. Returns
    /// the list of plugins that were loaded.</summary>
    public Plugin[] LoadPlugins(string[] files)
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        using var pinned = new Bridge.PinnedUtf8Array(files);
        var count = T->PluginManagerLoadPluginsFiles(_ptr, pinned.Pointers, files.Length, buffer, capacity);
        var plugins = new Plugin[count];
        for (var i = 0; i < count; i++)
        {
            plugins[i] = Plugin.FromNative((IntPtr)buffer[i]);
        }
        return plugins;
    }
}