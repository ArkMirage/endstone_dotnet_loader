namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Dimension.</summary>
public sealed unsafe class Dimension
{
    private readonly void* _ptr;

    internal Dimension(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Name => Bridge.Str(T->DimensionGetName(_ptr));
    public DimensionType Type => (DimensionType)T->DimensionGetType(_ptr);
    public Level Level => new((IntPtr)T->DimensionGetLevel(_ptr));

    /// <summary>Gets the block at the given block coordinates. Caller owns the returned block (Dispose it).</summary>
    public Block? GetBlockAt(int x, int y, int z)
    {
        var b = T->DimensionGetBlockAt(_ptr, x, y, z);
        return b == null ? null : new Block((IntPtr)b, ownsPtr: true);
    }

    /// <summary>
    /// Gets the block at the given location. Mirrors the native
    /// Dimension::getBlockAt(Location) overload, which uses the floored coordinates.
    /// Caller owns the returned block (Dispose it).
    /// </summary>
    public Block? GetBlockAt(Location location) =>
        GetBlockAt(location.GetBlockX(), location.GetBlockY(), location.GetBlockZ());

    public int GetHighestBlockYAt(int x, int z) => T->DimensionGetHighestBlockYAt(_ptr, x, z);

    /// <summary>Gets the highest block at the given coordinates. Caller owns the returned block (Dispose it).</summary>
    public Block? GetHighestBlockAt(int x, int z)
    {
        var b = T->DimensionGetHighestBlockAt(_ptr, x, z);
        return b == null ? null : new Block((IntPtr)b, ownsPtr: true);
    }

    /// <summary>
    /// Gets the highest block at the given location. Mirrors the native
    /// Dimension::getHighestBlockAt(Location) overload. Caller owns the returned block (Dispose it).
    /// </summary>
    public Block? GetHighestBlockAt(Location location) =>
        GetHighestBlockAt(location.GetBlockX(), location.GetBlockZ());

    /// <summary>Gets all loaded chunks. Each chunk is owned by the returned wrapper (Dispose it).</summary>
    public Chunk[] GetLoadedChunks()
    {
        const int capacity = 1024;
        var buffer = stackalloc void*[capacity];
        var count = T->DimensionGetLoadedChunks(_ptr, buffer, capacity);
        var chunks = new Chunk[count];
        for (var i = 0; i < count; i++)
        {
            chunks[i] = new Chunk((IntPtr)buffer[i], ownsPtr: true);
        }
        return chunks;
    }

    public Actor[] GetActors()
    {
        const int capacity = 1024;
        var buffer = stackalloc void*[capacity];
        var count = T->DimensionGetActors(_ptr, buffer, capacity);
        var actors = new Actor[count];
        for (var i = 0; i < count; i++)
        {
            actors[i] = new Actor((IntPtr)buffer[i]);
        }
        return actors;
    }

    /// <summary>Spawns an actor of the given type (e.g. "minecraft:zombie") at the location. Returns null if failed.</summary>
    public Actor? SpawnActor(string type, Location location)
    {
        var values = stackalloc float[5] { location.X, location.Y, location.Z, location.Pitch, location.Yaw };
        var buf = System.Text.Encoding.UTF8.GetBytes(type + "\0");
        fixed (byte* p = buf)
        {
            var a = T->DimensionSpawnActor(_ptr, values, p);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }

    /// <summary>Drops the item stack at the location. The returned wrapper reads the
    /// resulting item entity (call RemoveFromWorld after pickup-related cleanup no longer needed).</summary>
    public ItemStack DropItem(Location location, ItemStack item)
    {
        var values = stackalloc float[5] { location.X, location.Y, location.Z, location.Pitch, location.Yaw };
        var i = T->DimensionDropItem(_ptr, values, (void*)item.NativePtr);
        return i == null ? throw new InvalidOperationException("Failed to drop item") : new ItemStack((IntPtr)i, isItemActor: true);
    }
}
