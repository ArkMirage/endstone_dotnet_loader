namespace Endstone.Loader;

/// <summary>Wraps a native endstone::ItemType registry entry. The server owns
/// the entry; this wrapper is a non-owning view.</summary>
public sealed unsafe class ItemType
{
    private readonly void* _ptr;

    internal ItemType(IntPtr ptr) => _ptr = (void*)ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>Looks up an item type by identifier (for example
    /// "minecraft:diamond"), or returns null when the type is unknown.</summary>
    public static ItemType? Get(string id)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(id + "\0");
        fixed (byte* p = buf)
        {
            var type = T->ItemTypeGet(p);
            return type == null ? null : new ItemType((IntPtr)type);
        }
    }

    /// <summary>Full identifier, for example "minecraft:diamond".</summary>
    public string Id => Bridge.Str(T->ItemTypeGetId(_ptr));

    /// <summary>Translation key for this item type.</summary>
    public string TranslationKey => Bridge.Str(T->ItemTypeGetTranslationKey(_ptr));

    /// <summary>Translation key for a specific data value.</summary>
    public string GetTranslationKey(int data) =>
        Bridge.Str(T->ItemTypeGetTranslationKeyData(_ptr, data));

    public int MaxStackSize => T->ItemTypeGetMaxStackSize(_ptr);

    public int MaxDurability => T->ItemTypeGetMaxDurability(_ptr);

    /// <summary>Creates a new item stack of this type. The caller owns the
    /// returned stack and should dispose it.</summary>
    public ItemStack CreateItemStack(int amount = 1)
    {
        var stack = T->ItemTypeCreateItemStack(_ptr, amount);
        return new ItemStack((IntPtr)stack, ownsNative: true);
    }

    public override string ToString() => Id;
}
