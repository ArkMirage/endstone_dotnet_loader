using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Endstone.Loader;

/// <summary>
/// Non-collectible ALC hosting "shared contract" assemblies (interfaces,
/// abstract base classes, DTOs, enums) that plugins expose to each other.
/// Loading them here yields one Type identity server-wide, so an object
/// obtained from another plugin (via GetPlugin / GetService) can be cast to
/// its concrete shared type from any plugin's ALC.
///
/// The context is intentionally not collectible: shared types must outlive any
/// single plugin so that references held by other plugins never dangle onto a
/// reclaimed Type.
/// </summary>
internal sealed class SharedLoadContext : AssemblyLoadContext
{
    /// <summary>The single shared context instance, created once at startup.</summary>
    internal static SharedLoadContext Instance { get; } = new();

    // Simple-name -> loaded assembly. Case-insensitive so a plugin referencing
    // "MyContract" resolves to the same instance as "mycontract".
    private static readonly ConcurrentDictionary<string, Assembly> Loaded =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly List<string> _searchDirs = new();

    private SharedLoadContext() : base("endstone-shared", isCollectible: false) { }

    /// <summary>Registers a directory scanned when resolving intra-shared
    /// dependencies (and, for the base dir, top-level *.API.dll files).</summary>
    public void AddSearchDirectory(string dir)
    {
        if (Directory.Exists(dir) && !_searchDirs.Contains(dir))
        {
            _searchDirs.Add(dir);
        }
    }

    /// <summary>Loads a contract assembly into the shared context, returning the
    /// already-loaded instance when the simple name is already present.</summary>
    public Assembly LoadShared(string path)
    {
        var full = Path.GetFullPath(path);
        var name = AssemblyName.GetAssemblyName(full).Name ?? throw new BadImageFormatException($"Assembly '{full}' has no name.");
        if (Loaded.TryGetValue(name, out var existing))
        {
            return existing;
        }
        var asm = LoadFromAssemblyPath(full);
        Loaded[name] = asm;
        return asm;
    }

    /// <summary>Resolves a shared assembly by simple name, or null to let the
    /// default context handle non-shared references (e.g. Endstone.Loader, BCL).</summary>
    public static bool TryGetShared(AssemblyName name, out Assembly? assembly) =>
        Loaded.TryGetValue(name.Name!, out assembly);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Resolve intra-shared dependencies from the same shared tree so a
        // shared assembly's own references keep a single Type identity instead
        // of being pulled into the default context as a second copy.
        foreach (var dir in _searchDirs)
        {
            var candidate = Path.Combine(dir, assemblyName.Name + ".dll");
            if (File.Exists(candidate))
            {
                return LoadShared(candidate);
            }
        }
        return null;
    }
}
