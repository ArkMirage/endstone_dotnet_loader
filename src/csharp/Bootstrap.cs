using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Endstone.Loader;

/// <summary>
/// Native-callable entry points used by the C++ dotnet_loader plugin.
/// Every plugin assembly is loaded into its own collectible AssemblyLoadContext
/// so plugins can carry their own dependencies.
/// </summary>
public static class Bootstrap
{
    private static unsafe delegate* unmanaged[Cdecl]<void*, int, byte*, void> _logFn;

    private static IntPtr _serverPtr;

    // Shared serializer options: the native side parses plugin info as JSON with
    // camelCase keys, e.g. {"name","version","description","authors","commands"}.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal static IntPtr ServerPtr => _serverPtr;

    internal static unsafe void Log(IntPtr nativePlugin, LogLevel level, string message)
    {
        if (_logFn == null)
        {
            return;
        }
        var bytes = Encoding.UTF8.GetBytes(message);
        var buf = new byte[bytes.Length + 1];
        bytes.CopyTo(buf, 0);
        fixed (byte* pz = buf)
        {
            _logFn((void*)nativePlugin, (int)level, pz);
        }
    }

    [UnmanagedCallersOnly]
    public static unsafe int Init(IntPtr logFn, IntPtr bridgeTable)
    {
        _logFn = (delegate* unmanaged[Cdecl]<void*, int, byte*, void>)logFn;
        Bridge.Initialize(bridgeTable);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log(IntPtr.Zero, LogLevel.Critical, $"Unhandled exception in .NET plugin: {e.ExceptionObject}");
        return 0;
    }

    [UnmanagedCallersOnly]
    public static void SetServer(IntPtr serverPtr) => _serverPtr = serverPtr;

    /// <summary>
    /// Loads a plugin assembly, finds the [Plugin]-annotated PluginBase subclass,
    /// instantiates it and returns a GCHandle. Writes JSON plugin info
    /// ({"name","version","description","authors","commands"}) — or an error
    /// message on failure — into the caller-provided UTF-8 buffer.
    /// </summary>
    [UnmanagedCallersOnly]
    public static IntPtr LoadPlugin(IntPtr assemblyPathUtf8, IntPtr infoBuffer, int bufferSize)
    {
        string path = Marshal.PtrToStringUTF8(assemblyPathUtf8) ?? "";
        try
        {
            var fullPath = Path.GetFullPath(path);
            var alc = new PluginLoadContext(fullPath);
            var assembly = alc.LoadFromAssemblyPath(fullPath);

            Type? pluginType = null;
            PluginAttribute? meta = null;
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && typeof(PluginBase).IsAssignableFrom(type))
                {
                    meta = type.GetCustomAttribute<PluginAttribute>();
                    if (meta != null)
                    {
                        pluginType = type;
                        break;
                    }
                }
            }

            if (pluginType == null || meta == null)
            {
                WriteUtf8(infoBuffer, bufferSize, "no [Plugin] class extending PluginBase found");
                return IntPtr.Zero;
            }

            var instance = (PluginBase)Activator.CreateInstance(pluginType)!;
            instance.Description = PluginDescription.FromAttribute(meta);

            var info = JsonSerializer.Serialize(
                // New metadata fields are temporarily filled with the C++ side's
                // default values (empty collections / empty strings) until real
                // values are wired through PluginAttribute.
                new PluginInfo(meta.Name, meta.Version, meta.Description, meta.Authors,
                                meta.Contributors, meta.Website, meta.Prefix,
                                meta.Depend, meta.SoftDepend, meta.LoadBefore,
                                meta.DefaultPermission,
                                instance.CommandDefinitions.ToArray()),
                JsonOptions);

            WriteUtf8(infoBuffer, bufferSize, info);
            return GCHandle.ToIntPtr(GCHandle.Alloc(instance));
        }
        catch (Exception e)
        {
            WriteUtf8(infoBuffer, bufferSize, e.ToString());
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// Loads a plugin through a managed <see cref="PluginLoader"/> instance
    /// (registered via PluginManager.RegisterLoader). Returns a GCHandle to the
    /// PluginBase it produces, or 0 on failure / when the file is skipped.
    /// </summary>
    [UnmanagedCallersOnly]
    public static IntPtr LoadPluginViaLoader(IntPtr loaderGcHandle, IntPtr fileUtf8, IntPtr infoBuffer, int bufferSize)
    {
        var path = Marshal.PtrToStringUTF8(fileUtf8) ?? "";
        try
        {
            if (GCHandle.FromIntPtr(loaderGcHandle).Target is not PluginLoader loader)
            {
                WriteUtf8(infoBuffer, bufferSize, "invalid loader handle");
                return IntPtr.Zero;
            }

            var instance = loader.LoadPlugin(path);
            if (instance is null)
            {
                WriteUtf8(infoBuffer, bufferSize, "");  // empty info => skip silently
                return IntPtr.Zero;
            }

            var d = instance.Description;
            var info = JsonSerializer.Serialize(
                new PluginInfo(d.Name, d.Version, d.Description, d.Authors, d.Contributors,
                                d.Website, d.Prefix, d.Depend, d.SoftDepend, d.LoadBefore,
                                d.DefaultPermission, instance.CommandDefinitions.ToArray()),
                JsonOptions);
            WriteUtf8(infoBuffer, bufferSize, info);
            return GCHandle.ToIntPtr(GCHandle.Alloc(instance));
        }
        catch (Exception e)
        {
            WriteUtf8(infoBuffer, bufferSize, e.ToString());
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// Serializes a loader's <see cref="PluginLoader.FileFilters"/> as a JSON
    /// array of strings into the caller-provided buffer; returns 1 on success.
    /// Only structural checks (non-empty list, non-empty strings) happen here;
    /// regex syntax is validated natively with std::regex — the same engine
    /// endstone uses — so no pattern is rejected or accepted by a mismatched
    /// engine. Failures are logged with the loader's assembly/type so the log
    /// identifies which plugin's loader implementation is broken.
    /// </summary>
    [UnmanagedCallersOnly]
    public static unsafe int GetLoaderFilters(IntPtr loaderGcHandle, IntPtr buffer, int bufferSize)
    {
        PluginLoader? loader = null;
        try
        {
            if (GCHandle.FromIntPtr(loaderGcHandle).Target is not PluginLoader l)
            {
                Log(IntPtr.Zero, LogLevel.Error, "Failed to query custom loader file filters: invalid loader handle.");
                return 0;
            }
            loader = l;
            var filters = loader.FileFilters ?? [];
            if (filters.Count == 0)
            {
                Log(IntPtr.Zero, LogLevel.Error,
                    $"Failed to query custom loader file filters from '{LoaderName(loader)}': no file filters declared.");
                return 0;
            }
            foreach (var f in filters)
            {
                if (string.IsNullOrEmpty(f))
                {
                    Log(IntPtr.Zero, LogLevel.Error,
                        $"Failed to query custom loader file filters from '{LoaderName(loader)}': empty filter string.");
                    return 0;
                }
                // Regex syntax is validated natively with std::regex — the same
                // engine endstone uses — so no pattern is rejected or accepted
                // by a mismatched engine.
                var buf = Bridge.ToUtf8(f);
                fixed (byte* p = buf)
                {
                    if (!Bridge.Raw->ValidateRegex(p))
                    {
                        Log(IntPtr.Zero, LogLevel.Error,
                            $"Failed to query custom loader file filters from '{LoaderName(loader)}': invalid filter '{f}'.");
                        return 0;
                    }
                }
            }
            var json = JsonSerializer.Serialize(filters, JsonOptions);
            WriteUtf8(buffer, bufferSize, json);
            return 1;
        }
        catch (Exception e)
        {
            Log(IntPtr.Zero, LogLevel.Error,
                $"Failed to query custom loader file filters from '{LoaderName(loader)}': {e}");
            return 0;
        }
    }

    // The assembly name (e.g. "MyPlugin" for MyPlugin.Plugin.dll) is the
    // closest zero-cost proxy for the plugin name; the loader type alone
    // cannot identify the owning plugin.
    private static string LoaderName(PluginLoader? loader) =>
        loader is null ? "unknown loader" : $"{loader.GetType().Assembly.GetName().Name} ({loader.GetType().FullName})";

    record class PluginInfo(string Name, string Version, string Description, string[] Authors, string[] Contributors,
        string Website, string Prefix, string[] Depend, string[] SoftDepend, string[] LoadBefore,
        PermissionDefault DefaultPermission, CommandDefinition[] Commands);

    private static readonly Dictionary<IntPtr, PluginBase> Plugins = new();

    /// <summary>Maps a native Plugin* back to its managed PluginBase, or null
    /// when the plugin is not (or no longer) loaded.</summary>
    internal static PluginBase? FindPlugin(IntPtr nativePtr)
    {
        if (nativePtr == IntPtr.Zero)
        {
            return null;
        }
        lock (Plugins)
        {
            return Plugins.TryGetValue(nativePtr, out var plugin) ? plugin : null;
        }
    }

    [UnmanagedCallersOnly]
    public static void Attach(IntPtr gcHandle, IntPtr nativePlugin)
    {
        if (Resolve(gcHandle) is { } plugin)
        {
            plugin.SetPluginHandles(gcHandle, nativePlugin);
            lock (Plugins)
            {
                Plugins[nativePlugin] = plugin;
            }
        }
    }

    [UnmanagedCallersOnly]
    public static void OnLoad(IntPtr gcHandle) => Invoke(gcHandle, p =>
    {
        p.RegisterPermissions();
        p.OnLoad();
    });

    [UnmanagedCallersOnly]
    public static void OnEnable(IntPtr gcHandle) => Invoke(gcHandle, p => p.OnEnable());

    [UnmanagedCallersOnly]
    public static void OnDisable(IntPtr gcHandle) => Invoke(gcHandle, p =>
    {
        p.Scheduler.CancelAll();
        p.ServiceManager.UnregisterAll();
        p.OnDisable();
    });

    [UnmanagedCallersOnly]
    public static void Release(IntPtr gcHandle)
    {
        if (gcHandle != IntPtr.Zero)
        {
            if (Resolve(gcHandle) is { } plugin)
            {
                lock (Plugins)
                {
                    Plugins.Remove(plugin.NativeHandle);
                }
            }
            GCHandle.FromIntPtr(gcHandle).Free();
        }
    }

    /// <summary>Native event handler entry: dispatches to the managed callback.</summary>
    [UnmanagedCallersOnly]
    public static void DispatchEvent(IntPtr gcHandle, IntPtr cbHandle, IntPtr eventPtr)
    {
        if (cbHandle == IntPtr.Zero)
        {
            return;
        }
        try
        {
            var callback = (Action<IntPtr>?)GCHandle.FromIntPtr(cbHandle).Target;
            callback?.Invoke(eventPtr);
        }
        catch (Exception e)
        {
            Log(IntPtr.Zero, LogLevel.Error, $"Error in managed event handler: \n {e}");
        }
    }

    /// <summary>Native form callback entry: dispatches submit/close to the managed form.</summary>
    [UnmanagedCallersOnly]
    public static void FormDispatch(IntPtr playerPtr, int resultKind, ulong formId, int buttonIndex,
                                    IntPtr payloadUtf8)
    {
        try
        {
            if (FormRegistry.Take((long)formId) is not { } form)
            {
                return;
            }
            var player = new Player(playerPtr);
            if (resultKind == 1)
            {
                form.InvokeClose(player);
            }
            else
            {
                form.InvokeSubmit(player, buttonIndex, Marshal.PtrToStringUTF8(payloadUtf8) ?? "");
            }
        }
        catch (Exception e)
        {
            Log(IntPtr.Zero, LogLevel.Error, $"Error in managed form handler: {e}");
        }
    }

    /// <summary>Re-queries command declarations (called by native side after OnLoad).
    /// Writes a JSON array of command definitions; returns 1 when the buffer was
    /// written, 0 when the plugin handle is unknown.</summary>
    [UnmanagedCallersOnly]
    public static int QueryCommands(IntPtr gcHandle, IntPtr buffer, int bufferSize)
    {
        if (Resolve(gcHandle) is not { } plugin)
        {
            return 0;
        }
        var info = JsonSerializer.Serialize(plugin.CommandDefinitions, JsonOptions);
        WriteUtf8(buffer, bufferSize, info);
        return 1;
    }

    /// <summary>Native command entry: dispatches to the managed command handler.</summary>
    [UnmanagedCallersOnly]
    public static int DispatchCommand(IntPtr gcHandle, IntPtr senderPtr, IntPtr commandNameUtf8, IntPtr argsUtf8,
                                      int argCount)
    {
        if (Resolve(gcHandle) is not { } plugin)
        {
            return 0;
        }
        try
        {
            var name = Marshal.PtrToStringUTF8(commandNameUtf8) ?? "";
            var args = new string[Math.Max(argCount, 0)];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(argsUtf8, i * IntPtr.Size)) ?? "";
            }
            return plugin.DispatchCommand(name, new CommandSender(senderPtr), args) ? 1 : 0;
        }
        catch (Exception e)
        {
            Log(plugin.NativeHandle, LogLevel.Error, $"Error in managed command handler: {e}");
            return 0;
        }
    }

    private static PluginBase? Resolve(IntPtr gcHandle) =>
        gcHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(gcHandle).Target as PluginBase;

    private static void Invoke(IntPtr gcHandle, Action<PluginBase> action)
    {
        if (Resolve(gcHandle) is not { } plugin)
        {
            return;
        }
        try
        {
            action(plugin);
        }
        catch (Exception e)
        {
            Log(plugin.NativeHandle, LogLevel.Error, e.ToString());
        }
    }

    private static unsafe void WriteUtf8(IntPtr buffer, int bufferSize, string text)
    {
        if (buffer == IntPtr.Zero || bufferSize <= 0)
        {
            return;
        }
        var span = new Span<byte>((void*)buffer, bufferSize);
        int maxChars = Math.Min(text.Length, (bufferSize - 1) / 4);
        int written = Encoding.UTF8.GetBytes(text.AsSpan(0, maxChars), span[..^1]);
        span[written] = 0;
    }

    /// <summary>Native map render entry: forwards endstone's render() to the managed renderer.</summary>
    [UnmanagedCallersOnly]
    public static void MapRenderDispatch(IntPtr canvasPtr, IntPtr mapPtr, IntPtr playerPtr, ulong rendererId)
    {
        try
        {
            if (MapRenderer.Find(rendererId) is not { } renderer)
            {
                return;
            }
            renderer.Render(new MapView(mapPtr), new MapCanvas(canvasPtr), new Player(playerPtr));
        }
        catch (Exception e)
        {
            Log(IntPtr.Zero, LogLevel.Error, $"Error in managed map renderer: {e}");
        }
    }

    /// <summary>Native scheduler task entry: fires the managed callback by managed task id.</summary>
    [UnmanagedCallersOnly]
    public static void TaskDispatch(ulong managedTaskId)
    {
        try
        {
            Scheduler.Fire(managedTaskId);
        }
        catch (Exception e)
        {
            Log(IntPtr.Zero, LogLevel.Error, $"Error in managed scheduler task: {e}");
        }
    }
}
