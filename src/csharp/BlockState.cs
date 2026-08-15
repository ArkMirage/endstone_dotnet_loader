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
            return new Location(values[0], values[1], values[2], values[3], values[4]);
        }
    }

    public void SetType(string type) => Bridge.Call1(T->BlockStateSetType, _ptr, type);

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