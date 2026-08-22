using System.Reflection;
using System.Runtime.Loader;

namespace Endstone.Loader;

/// <summary>
/// Per-plugin AssemblyLoadContext. Resolves plugin dependencies from the
/// plugin's own directory (via its .deps.json), while sharing Endstone.Loader
/// itself with the default context so PluginBase type identity is preserved.
/// </summary>
internal sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(Path.GetFileName(pluginPath))
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Types from the loader API assembly must come from the default context,
        // otherwise `typeof(PluginBase).IsAssignableFrom(...)` would fail.
        if (assemblyName.Name == typeof(Bootstrap).Assembly.GetName().Name)
        {
            return typeof(Bootstrap).Assembly;
        }

        // Shared contract assemblies resolve to the single shared ALC instance
        // so their Type identity is server-wide and cross-ALC casts succeed.
        if (SharedLoadContext.TryGetShared(assemblyName, out var shared))
        {
            return shared;
        }

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path != null ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
    }
}
