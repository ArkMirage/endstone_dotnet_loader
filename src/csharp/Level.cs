namespace Endstone.Loader;

public enum DimensionType
{
    Overworld = 0,
    Nether = 1,
    TheEnd = 2,
    Custom = 999
}

/// <summary>Wraps a native endstone::Level.</summary>
public sealed unsafe class Level
{
    private readonly void* _ptr;

    internal Level(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Name => Bridge.Str(T->LevelGetName(_ptr));
    public int Time
    {
        get => T->LevelGetTime(_ptr);
        set => T->LevelSetTime(_ptr, value);
    }
    public long Seed => T->LevelGetSeed(_ptr);

    public Actor[] GetActors()
    {
        const int capacity = 1024;
        var buffer = stackalloc void*[capacity];
        var count = T->LevelGetActors(_ptr, buffer, capacity);
        var actors = new Actor[count];
        for (var i = 0; i < count; i++)
        {
            actors[i] = new Actor((IntPtr)buffer[i]);
        }
        return actors;
    }

    public Dimension[] GetDimensions()
    {
        const int capacity = 64;
        var buffer = stackalloc void*[capacity];
        var count = T->LevelGetDimensions(_ptr, buffer, capacity);
        var dimensions = new Dimension[count];
        for (var i = 0; i < count; i++)
        {
            dimensions[i] = new Dimension((IntPtr)buffer[i]);
        }
        return dimensions;
    }

    public Dimension? GetDimension(string name)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(name + "\0");
        fixed (byte* p = buf)
        {
            var dimension = T->LevelGetDimensionByName(_ptr, p);
            return dimension == null ? null : new Dimension((IntPtr)dimension);
        }
    }
}

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

    public int GetHighestBlockYAt(int x, int z) => T->DimensionGetHighestBlockYAt(_ptr, x, z);

    /// <summary>Gets the highest block at the given coordinates. Caller owns the returned block (Dispose it).</summary>
    public Block? GetHighestBlockAt(int x, int z)
    {
        var b = T->DimensionGetHighestBlockAt(_ptr, x, z);
        return b == null ? null : new Block((IntPtr)b, ownsPtr: true);
    }

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

/// <summary>Wraps a native endstone::Chunk. Owns the native chunk when created from GetLoadedChunks.</summary>
public sealed unsafe class Chunk : IDisposable
{
    private void* _ptr;
    private readonly bool _ownsPtr;

    internal Chunk(IntPtr ptr, bool ownsPtr = false)
    {
        _ptr = (void*)ptr;
        _ownsPtr = ownsPtr;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public int X => T->ChunkObjGetX(_ptr);
    public int Z => T->ChunkObjGetZ(_ptr);
    public Dimension Dimension => new((IntPtr)T->ChunkObjGetDimension(_ptr));

    public void Dispose()
    {
        if (_ownsPtr && _ptr != null)
        {
            T->ChunkObjDelete(_ptr);
            _ptr = null;
        }
    }
}