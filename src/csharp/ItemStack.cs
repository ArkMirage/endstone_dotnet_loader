using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Endstone.Loader.Nbt;

namespace Endstone.Loader;

/// <summary>Kind of metadata an item stack carries (endstone::ItemMeta::Type).</summary>
public enum ItemMetaType
{
    Item = 0,
    Book = 1,
    Crossbow = 2,
    Map = 3,
    WritableBook = 4,
}

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

    public string Type => Bridge.Str(T->ItemGetType(_ptr));
    public int Amount => T->ItemGetAmount(_ptr);
    public int Data => T->ItemGetData(_ptr);
    public int MaxStackSize => T->ItemGetMaxStackSize(_ptr);
    public string TranslationKey => Bridge.Str(T->ItemGetTranslationKey(_ptr));

    /// <summary>ItemMeta: display name, or empty when unset.
    /// Setting null or an empty string clears it. The native side reports whether
    /// the change was applied; a property setter cannot surface that.</summary>
    public bool HasDisplayName => T->ItemHasDisplayName(_ptr);
    [AllowNull]
    public string DisplayName
    {
        get => Bridge.Str(T->ItemGetDisplayName(_ptr));
        set => Bridge.CallBoolStrOrNull(T->ItemSetDisplayName, _ptr, value);
    }

    public bool HasLore => T->ItemHasLore(_ptr);
    public int LoreCount => T->ItemGetLoreCount(_ptr);
    public string GetLoreLine(int index) => Bridge.Str(T->ItemGetLoreLine(_ptr, index));

    /// <summary>All lore lines as a snapshot (empty when no lore is set). Setting
    /// null or an empty list clears the lore.</summary>
    [AllowNull]
    public IReadOnlyList<string> Lore
    {
        get
        {
            var lines = new List<string>(LoreCount);
            for (var i = 0; i < LoreCount; i++)
            {
                lines.Add(GetLoreLine(i));
            }
            return lines;
        }
        set => Bridge.CallItemSetLore(T->ItemSetLore, _ptr, value);
    }

    public bool HasDamage => T->ItemHasDamage(_ptr);
    public int Damage
    {
        get => T->ItemGetDamage(_ptr);
        set => T->ItemSetDamage(_ptr, value);
    }

    public bool IsUnbreakable
    {
        get => T->ItemIsUnbreakable(_ptr);
        set => T->ItemSetUnbreakable(_ptr, value);
    }

    /// <summary>Whether the item carries a repair penalty.</summary>
    public bool HasRepairCost => T->ItemHasRepairCost(_ptr);

    /// <summary>The repair penalty, or 0 when unset.</summary>
    public int RepairCost
    {
        get => T->ItemGetRepairCost(_ptr);
        set => T->ItemSetRepairCost(_ptr, value);
    }

    /// <summary>Kind of metadata the item carries, or null when it carries none.</summary>
    public ItemMetaType? MetaType
    {
        get
        {
            var type = T->ItemGetMetaType(_ptr);
            return type < 0 ? null : (ItemMetaType)type;
        }
    }

    public bool HasEnchants => T->ItemHasEnchants(_ptr);
    public int EnchantCount => T->ItemGetEnchantCount(_ptr);
    public string GetEnchantName(int index) => Bridge.Str(T->ItemGetEnchantName(_ptr, index));
    public int GetEnchantLevel(int index) => T->ItemGetEnchantLevel(_ptr, index);

    /// <summary>Whether the item carries the given enchantment (id like "minecraft:sharpness").</summary>
    public bool HasEnchant(string id) => Bridge.CallBoolStr(T->ItemHasEnchant, _ptr, id);

    /// <summary>Gets the level of the given enchantment, or 0 when absent.</summary>
    public int GetEnchantLevel(string id) => Bridge.CallIntStr(T->ItemGetEnchantLevelById, _ptr, id);

    /// <summary>Adds the enchantment at the given level. When force is false the
    /// level must be within the enchantment's start..max range. Conflicts with
    /// existing enchantments are not checked; use HasConflictingEnchant first.</summary>
    public bool AddEnchant(string id, int level, bool force = false)
        => Bridge.CallBoolStrInt(T->ItemAddEnchant, _ptr, id, level, force);

    /// <summary>Removes the enchantment if present. Returns true if it was removed.</summary>
    public bool RemoveEnchant(string id) => Bridge.CallBoolStr(T->ItemRemoveEnchant, _ptr, id);

    /// <summary>Removes all enchantments from the item.</summary>
    public void RemoveEnchants() => Bridge.CallVoidStr(T->ItemRemoveEnchants, _ptr, "");

    /// <summary>Whether any enchantment on the item conflicts with the given one.</summary>
    public bool HasConflictingEnchant(string id)
        => Bridge.CallBoolStr(T->ItemHasConflictingEnchant, _ptr, id);

    /// <summary>All enchantments applied to the item, as (enchantment, level) pairs.</summary>
    public IReadOnlyList<ItemEnchantment> Enchantments
    {
        get
        {
            var list = new List<ItemEnchantment>(EnchantCount);
            for (var i = 0; i < EnchantCount; i++)
            {
                var id = GetEnchantName(i);
                if (Enchantment.Get(id) is { } enchant)
                {
                    list.Add(new ItemEnchantment(enchant, GetEnchantLevel(i)));
                }
            }
            return list;
        }
    }

    /// <summary>Map meta: whether this item is a map bound to a server map view.</summary>
    public bool HasMapView => T->ItemHasMapView(_ptr);

    /// <summary>Gets the map view bound to this map item, or null.</summary>
    public MapView? MapView
    {
        get
        {
            var m = T->ItemGetMapView(_ptr);
            return m == null ? null : new MapView((IntPtr)m);
        }
    }

    /// <summary>Binds this map item to the given map view (only works on map item stacks).</summary>
    public bool SetMapView(MapView map) => T->ItemSetMapView(_ptr, (void*)map.NativePtr);

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
