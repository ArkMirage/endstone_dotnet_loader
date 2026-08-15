namespace Endstone.Loader;

/// <summary>Wraps a native endstone::DamageSource.</summary>
public sealed unsafe class DamageSource
{
    private readonly void* _ptr;

    internal DamageSource(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Type => Bridge.Str(T->DamageSourceGetType(_ptr));
    public bool IsIndirect => T->DamageSourceIsIndirect(_ptr);

    public Actor? Actor
    {
        get
        {
            var a = T->DamageSourceGetActor(_ptr);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }

    public Actor? DamagingActor
    {
        get
        {
            var a = T->DamageSourceGetDamagingActor(_ptr);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }
}