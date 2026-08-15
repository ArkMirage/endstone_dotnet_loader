namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Block.</summary>
public sealed unsafe class Block : IDisposable
{
    private void* _ptr;
    private readonly bool _ownsPtr;

    internal Block(IntPtr ptr, bool ownsPtr = false)
    {
        _ptr = (void*)ptr;
        _ownsPtr = ownsPtr;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Type => Bridge.Str(T->BlockGetType(_ptr));
    public int X => T->BlockGetX(_ptr);
    public int Y => T->BlockGetY(_ptr);
    public int Z => T->BlockGetZ(_ptr);
    public string Dimension => Bridge.Str(T->BlockGetDimensionName(_ptr));

    public Location Location
    {
        get
        {
            var values = stackalloc float[5];
            T->BlockGetLocation(_ptr, values);
            return new Location(values[0], values[1], values[2], values[3], values[4]);
        }
    }

    public void SetType(string type) => Bridge.Call1(T->BlockSetType, _ptr, type);

    public void SetType(string type, bool applyPhysics)
    {
        var buf = Bridge.ToUtf8(type);
        fixed (byte* p = buf)
        {
            T->BlockSetTypePhysics(_ptr, p, applyPhysics);
        }
    }

    /// <summary>Gets the block at the given offsets. Caller owns the returned block (Dispose it).</summary>
    public Block GetRelative(int offsetX, int offsetY, int offsetZ)
    {
        var b = T->BlockGetRelative(_ptr, offsetX, offsetY, offsetZ);
        return b == null ? throw new InvalidOperationException("Failed to get relative block") : new Block((IntPtr)b, ownsPtr: true);
    }

    /// <summary>Captures the current state of this block. Caller owns the returned state (Dispose it).</summary>
    public BlockState CaptureState()
    {
        var s = T->BlockCaptureState(_ptr);
        return s == null ? throw new InvalidOperationException("Failed to capture block state") : new BlockState((IntPtr)s, ownsPtr: true);
    }

    public void Dispose()
    {
        if (_ownsPtr && _ptr != null)
        {
            T->BlockDelete(_ptr);
            _ptr = null;
        }
    }
}