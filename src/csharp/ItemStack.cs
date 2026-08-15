namespace Endstone.Loader;

/// <summary>Wraps a native endstone::ItemStack read-only view. When created via
/// ItemStack.Create, Dispose() frees the native stack. When wrapping a dropped
/// item entity (isItemActor), RemoveFromWorld() removes it from the level.</summary>
public sealed unsafe class ItemStack : IDisposable
{
    private void* _ptr;
    private readonly bool _isItemActor;
    private readonly bool _ownsNative;

    internal ItemStack(IntPtr ptr, bool isItemActor = false, bool ownsNative = false)
    {
        _ptr = (void*)ptr;
        _isItemActor = isItemActor;
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

    /// <summary>Removes the dropped item entity from the world (only valid for DropItem results).</summary>
    public void RemoveFromWorld()
    {
        if (_isItemActor && _ptr != null)
        {
            T->ActorRemove(_ptr);
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

    public string Type => Bridge.Str(_isItemActor ? T->ItemActorGetType(_ptr) : T->ItemGetType(_ptr));
    public int Amount => _isItemActor ? T->ItemActorGetAmount(_ptr) : T->ItemGetAmount(_ptr);
    public int Data => _isItemActor ? 0 : T->ItemGetData(_ptr);
    public int MaxStackSize => _isItemActor ? 0 : T->ItemGetMaxStackSize(_ptr);
    public string TranslationKey => Bridge.Str(_isItemActor ? T->ItemActorGetTranslationKey(_ptr) : T->ItemGetTranslationKey(_ptr));

    /// <summary>ItemMeta: display name, or empty when unset (not available on item actors).</summary>
    public bool HasDisplayName => !_isItemActor && T->ItemHasDisplayName(_ptr);
    public string DisplayName => _isItemActor ? "" : Bridge.Str(T->ItemGetDisplayName(_ptr));
    public bool HasLore => !_isItemActor && T->ItemHasLore(_ptr);
    public int LoreCount => _isItemActor ? 0 : T->ItemGetLoreCount(_ptr);
    public string GetLoreLine(int index) => _isItemActor ? "" : Bridge.Str(T->ItemGetLoreLine(_ptr, index));
    public bool HasDamage => !_isItemActor && T->ItemHasDamage(_ptr);
    public int Damage => _isItemActor ? 0 : T->ItemGetDamage(_ptr);
    public bool IsUnbreakable => !_isItemActor && T->ItemIsUnbreakable(_ptr);
    public bool HasEnchants => !_isItemActor && T->ItemHasEnchants(_ptr);
    public int EnchantCount => _isItemActor ? 0 : T->ItemGetEnchantCount(_ptr);
    public string GetEnchantName(int index) => _isItemActor ? "" : Bridge.Str(T->ItemGetEnchantName(_ptr, index));
    public int GetEnchantLevel(int index) => _isItemActor ? 0 : T->ItemGetEnchantLevel(_ptr, index);

    /// <summary>Whether the item carries the given enchantment (id like "minecraft:sharpness").</summary>
    public bool HasEnchant(string id) => !_isItemActor && Bridge.CallBoolStr(T->ItemHasEnchant, _ptr, id);

    /// <summary>Gets the level of the given enchantment, or 0 when absent.</summary>
    public int GetEnchantLevel(string id) => _isItemActor ? 0 : Bridge.CallIntStr(T->ItemGetEnchantLevelById, _ptr, id);

    /// <summary>Adds the enchantment at the given level. When force is false the
    /// level must be within the enchantment's start..max range. Conflicts with
    /// existing enchantments are not checked; use HasConflictingEnchant first.</summary>
    public bool AddEnchant(string id, int level, bool force = false)
        => !_isItemActor && Bridge.CallBoolStrInt(T->ItemAddEnchant, _ptr, id, level, force);

    /// <summary>Removes the enchantment if present. Returns true if it was removed.</summary>
    public bool RemoveEnchant(string id) => !_isItemActor && Bridge.CallBoolStr(T->ItemRemoveEnchant, _ptr, id);

    /// <summary>Removes all enchantments from the item.</summary>
    public void RemoveEnchants()
    {
        if (!_isItemActor)
        {
            Bridge.CallVoidStr(T->ItemRemoveEnchants, _ptr, "");
        }
    }

    /// <summary>Whether any enchantment on the item conflicts with the given one.</summary>
    public bool HasConflictingEnchant(string id)
        => !_isItemActor && Bridge.CallBoolStr(T->ItemHasConflictingEnchant, _ptr, id);

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
    public bool HasMapView => !_isItemActor && T->ItemHasMapView(_ptr);

    /// <summary>Gets the map view bound to this map item, or null.</summary>
    public MapView? MapView
    {
        get
        {
            if (_isItemActor)
            {
                return null;
            }
            var m = T->ItemGetMapView(_ptr);
            return m == null ? null : new MapView((IntPtr)m);
        }
    }

    /// <summary>Binds this map item to the given map view (only works on map item stacks).</summary>
    public bool SetMapView(MapView map) => !_isItemActor && T->ItemSetMapView(_ptr, (void*)map.NativePtr);
}