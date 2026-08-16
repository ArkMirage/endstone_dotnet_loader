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

            var info = JsonSerializer.Serialize(
                // New metadata fields are temporarily filled with the C++ side's
                // default values (empty collections / empty strings) until real
                // values are wired through PluginAttribute.
                new PluginInfo(meta.Name, meta.Version, meta.Description, meta.Authors,
                               /*contributors=*/[], /*website=*/"", /*prefix=*/"",
                               /*depend=*/[], /*softDepend=*/[], /*loadBefore=*/[],
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

    record class PluginInfo(string Name, string Version, string Description, string[] Authors, string[] Contributors,
        string Website, string Prefix, string[] Depend, string[] SoftDepend, string[] LoadBefore,
        PermissionDefault DefaultPermission, CommandDefinition[] Commands);

    [UnmanagedCallersOnly]
    public static void Attach(IntPtr gcHandle, IntPtr nativePlugin)
    {
        if (Resolve(gcHandle) is { } plugin)
        {
            plugin.SetPluginHandles(gcHandle, nativePlugin);
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
