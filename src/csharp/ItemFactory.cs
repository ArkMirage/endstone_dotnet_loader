namespace Endstone.Loader;

/// <summary>
/// Wraps the native endstone::ItemFactory (server-owned; do not dispose).
/// Creates and compares ItemMeta instances for item types.
/// </summary>
public sealed unsafe class ItemFactory
{
    private readonly void* _ptr;

    internal ItemFactory(IntPtr ptr) => _ptr = (void*)ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>Creates a new, empty ItemMeta appropriate for the given item
    /// type (e.g. "minecraft:written_book" yields a BookMeta).</summary>
    public ItemMeta GetItemMeta(string type)
    {
        var meta = Bridge.CallPtrStr(T->ItemFactoryGetMeta, _ptr, type);
        return meta == null
            ? throw new InvalidOperationException($"Could not create item meta for '{type}'.")
            : ItemMeta.Wrap((IntPtr)meta);
    }

    /// <summary>Whether the meta can be applied to the given item type without
    /// losing data.</summary>
    public bool IsApplicable(ItemMeta? meta, string type)
    {
        var buf = Bridge.ToUtf8(type);
        fixed (byte* p = buf)
        {
            return T->ItemFactoryIsApplicable(_ptr, meta == null ? null : (void*)meta.NativePtr, p);
        }
    }

    /// <summary>Compares two metas. Two nulls are considered equal.</summary>
    public bool Equals(ItemMeta? first, ItemMeta? second)
        => T->ItemFactoryEquals(_ptr,
                                first == null ? null : (void*)first.NativePtr,
                                second == null ? null : (void*)second.NativePtr);

    /// <summary>Returns the most appropriate meta for the given item type,
    /// copying data from the supplied meta when applicable.</summary>
    public ItemMeta AsMetaFor(ItemMeta? meta, string type)
    {
        var buf = Bridge.ToUtf8(type);
        void* result;
        fixed (byte* p = buf)
        {
            result = T->ItemFactoryAsMetaFor(_ptr, meta == null ? null : (void*)meta.NativePtr, p);
        }
        return result == null
            ? throw new InvalidOperationException($"Could not convert item meta for '{type}'.")
            : ItemMeta.Wrap((IntPtr)result);
    }
}
