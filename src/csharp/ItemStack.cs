using System.Runtime.InteropServices;

using Endstone.Loader.Nbt;

namespace Endstone.Loader;

/// <summary>Wraps a native endstone::ItemStack. When created via
/// ItemStack.Create or ItemType.CreateItemStack, Dispose() frees the native
/// stack. Wrappers returned by getters are transient read-only views and must
/// not outlive the next item call on the same thread.</summary>
public sealed unsafe class ItemStack : IDisposable
{
    private void* _ptr;
    private readonly bool _ownsNative;

    internal ItemStack(IntPtr ptr, bool ownsNative = false)
    {
        _ptr = (void*)ptr;
        _ownsNative = ownsNative;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>Creates a new item stack of the given type (e.g. "minecraft:diamond"). Caller owns it (Dispose it).</summary>
    public static ItemStack? Create(string type, int amount = 1, int data = 0)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(type + "\0");
        fixed (byte* p = buf)
        {
            var s = T->ItemStackCreate(p, amount, data);
            return s == null ? null : new ItemStack((IntPtr)s, ownsNative: true);
        }
    }

    public void Dispose()
    {
        if (_ownsNative && _ptr != null)
        {
            T->ItemStackDelete(_ptr);
            _ptr = null;
        }
    }

    public string Type
    {
        get => Bridge.Str(T->ItemGetType(_ptr));
        set => Bridge.Call1(T->ItemSetType, _ptr, value);
    }

    public int Amount
    {
        get => T->ItemGetAmount(_ptr);
        set => T->ItemSetAmount(_ptr, value);
    }

    public int Data
    {
        get => T->ItemGetData(_ptr);
        set => T->ItemSetData(_ptr, value);
    }

    public int MaxStackSize => T->ItemGetMaxStackSize(_ptr);
    public string TranslationKey => Bridge.Str(T->ItemGetTranslationKey(_ptr));

    /// <summary>Whether the two stacks are equal ignoring amount
    /// (endstone::ItemStack::isSimilar).</summary>
    public bool IsSimilar(ItemStack other) => T->ItemIsSimilar(_ptr, (void*)other.NativePtr);

    /// <summary>Whether this stack carries any ItemMeta data.</summary>
    public bool HasItemMeta => T->ItemHasMeta(_ptr);

    /// <summary>Gets a copy of this stack's ItemMeta. The native side always
    /// returns a type-appropriate meta (use <see cref="HasItemMeta"/> to check
    /// whether it actually carries data). The returned wrapper owns the copy;
    /// dispose it (or let it be collected).</summary>
    public ItemMeta GetItemMeta() => ItemMeta.Wrap((IntPtr)T->ItemGetMeta(_ptr));

    /// <summary>Applies the given meta to this stack. A null meta clears it.
    /// Returns whether the native side accepted the meta.</summary>
    public bool SetItemMeta(ItemMeta? meta)
        => T->ItemSetMeta(_ptr, meta == null ? null : (void*)meta.NativePtr);

    /// <summary>Convenience accessor for <see cref="GetItemMeta"/>.</summary>
    public ItemMeta Meta => GetItemMeta();

    /// <summary>Gets the item's NBT compound tag, or null when the item has no NBT.
    /// The native side hands back already-serialized CBOR; we decode it into the
    /// managed NbtTag placeholder.</summary>
    public NbtTag? GetNbt()
    {
        int len = 0;
        var data = T->ItemGetNbt(_ptr, &len);
        if (len <= 0 || data == null)
        {
            return null;
        }
        var bytes = new byte[len];
        Marshal.Copy((IntPtr)data, bytes, 0, len);
        T->NbtFreeBuffer(data);
        return NbtCbor.Read(bytes);
    }

    /// <summary>Sets the item's NBT compound tag.</summary>
    public void SetNbt(NbtTag root)
    {
        var bytes = NbtCbor.Write(root);
        fixed (byte* p = bytes)
        {
            T->ItemSetNbt(_ptr, p, bytes.Length);
        }
    }
}
