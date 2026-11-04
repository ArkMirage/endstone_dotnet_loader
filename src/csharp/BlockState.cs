namespace Endstone.Loader;

/// <summary>Wraps a native endstone::BlockState.</summary>
public sealed unsafe class BlockState : IDisposable
{
    private void* _ptr;
    private readonly bool _ownsPtr;

    internal BlockState(IntPtr ptr, bool ownsPtr = false)
    {
        _ptr = (void*)ptr;
        _ownsPtr = ownsPtr;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>Gets the live block represented by this captured state.
    /// Caller owns the returned block (Dispose it).</summary>
    public Block GetBlock()
    {
        var b = T->BlockStateGetBlock(_ptr);
        return b == null ? throw new InvalidOperationException("Failed to get block from state") : new Block((IntPtr)b, ownsPtr: true);
    }

    public string Type => Bridge.Str(T->BlockStateGetType(_ptr));
    public int X => T->BlockStateGetX(_ptr);
    public int Y => T->BlockStateGetY(_ptr);
    public int Z => T->BlockStateGetZ(_ptr);

    public Location Location
    {
        get
        {
            var values = stackalloc float[5];
            T->BlockStateGetLocation(_ptr, values);
            var dim = T->BlockStateGetDimension(_ptr);
            var dimension = dim == null ? null : new Dimension((IntPtr)dim);
            return new Location(values[0], values[1], values[2], values[3], values[4], dimension);
        }
    }

    public void SetType(string type) => Bridge.Call1(T->BlockStateSetType, _ptr, type);

    /// <summary>Gets the block data for this captured state. Caller owns the
    /// returned wrapper (Dispose it).</summary>
    public BlockData? GetData()
    {
        var data = T->BlockStateGetData(_ptr);
        return data == null ? null : new BlockData((IntPtr)data, ownsPtr: true);
    }

    /// <summary>Changes the block data held by this captured state. The live
    /// block is not modified until <see cref="Update()"/> is called.</summary>
    public void SetData(BlockData data) => T->BlockStateSetData(_ptr, (void*)data.NativePtr);

    public bool Update() => T->BlockStateUpdate(_ptr);

    public bool Update(bool force) => T->BlockStateUpdateForce(_ptr, force);

    public bool Update(bool force, bool applyPhysics) => T->BlockStateUpdateForcePhysics(_ptr, force, applyPhysics);

    public void Dispose()
    {
        if (_ownsPtr && _ptr != null)
        {
            T->BlockStateDelete(_ptr);
            _ptr = null;
        }
    }
}