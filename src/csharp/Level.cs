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