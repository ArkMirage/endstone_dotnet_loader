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

/// <summary>Generation of a written book (endstone::BookMeta::Generation).</summary>
public enum BookGeneration
{
    Original = 0,
    CopyOfOriginal = 1,
    CopyOfCopy = 2,
}

/// <summary>
/// Wraps a native endstone::ItemMeta. Instances are owned copies: dispose them
/// (or let them be collected) when done. Obtain one from
/// <see cref="ItemStack.GetItemMeta"/> or <see cref="ItemFactory.GetItemMeta"/>.
/// The concrete subclass is chosen from the native meta type, so book/map/
/// crossbow-specific members are only available on the matching subclass.
/// </summary>
public unsafe class ItemMeta : IDisposable
{
    private void* _ptr;

    internal ItemMeta(IntPtr ptr) => _ptr = (void*)ptr;

    private protected static Bridge.Table* T => Bridge.Raw;

    protected void* Ptr => _ptr != null
        ? _ptr
        : throw new ObjectDisposedException(GetType().Name);

    internal IntPtr NativePtr => (IntPtr)Ptr;

    /// <summary>Wraps a native ItemMeta holder, selecting the concrete wrapper
    /// from its native type.</summary>
    internal static ItemMeta Wrap(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            throw new InvalidOperationException("The native item meta is null.");
        }
        var type = (ItemMetaType)T->ItemMetaGetType((void*)ptr);
        return type switch
        {
            ItemMetaType.Book => new BookMeta(ptr),
            ItemMetaType.WritableBook => new WritableBookMeta(ptr),
            ItemMetaType.Crossbow => new CrossbowMeta(ptr),
            ItemMetaType.Map => new MapMeta(ptr),
            _ => new ItemMeta(ptr),
        };
    }

    public ItemMetaType Type => (ItemMetaType)T->ItemMetaGetType(Ptr);

    public bool HasDisplayName => T->ItemMetaHasDisplayName(Ptr);

    /// <summary>The display name, or null when unset. Setting null clears it.</summary>
    public string? DisplayName
    {
        get => HasDisplayName ? Bridge.Str(T->ItemMetaGetDisplayName(Ptr)) : null;
        set => Bridge.CallVoidStrOrNull(T->ItemMetaSetDisplayName, Ptr, value);
    }

    public bool HasLore => T->ItemMetaHasLore(Ptr);

    /// <summary>All lore lines. Setting null or an empty list clears the lore.</summary>
    public IReadOnlyList<string> Lore
    {
        get
        {
            var count = T->ItemMetaGetLoreCount(Ptr);
            var lines = new string[count];
            for (var i = 0; i < count; i++)
            {
                lines[i] = Bridge.Str(T->ItemMetaGetLoreLine(Ptr, i));
            }
            return lines;
        }
        set => Bridge.CallVoidStrArray(T->ItemMetaSetLore, Ptr, value);
    }

    public bool HasEnchants => T->ItemMetaHasEnchants(Ptr);
    public int EnchantCount => T->ItemMetaGetEnchantCount(Ptr);

    public string GetEnchantName(int index) => Bridge.Str(T->ItemMetaGetEnchantName(Ptr, index));
    public int GetEnchantLevel(int index) => T->ItemMetaGetEnchantLevel(Ptr, index);

    /// <summary>All enchantments as (enchantment, level) pairs. Iteration order
    /// follows the native unordered map, so it is not guaranteed.</summary>
    public IReadOnlyList<ItemEnchantment> Enchants
    {
        get
        {
            var count = EnchantCount;
            var list = new List<ItemEnchantment>(count);
            for (var i = 0; i < count; i++)
            {
                var id = GetEnchantName(i);
                if (Enchantment.Get(id) is { } enchantment)
                {
                    list.Add(new ItemEnchantment(enchantment, GetEnchantLevel(i)));
                }
            }
            return list;
        }
    }

    /// <summary>Whether the meta carries the given enchantment (id like "minecraft:sharpness").</summary>
    public bool HasEnchant(string id) => Bridge.CallBoolStr(T->ItemMetaHasEnchant, Ptr, id);

    /// <summary>Gets the level of the given enchantment, or 0 when absent.</summary>
    public int GetEnchantLevel(string id) => Bridge.CallIntStr(T->ItemMetaGetEnchantLevelById, Ptr, id);

    /// <summary>Adds the enchantment at the given level. When force is false the
    /// level must be within the enchantment's start..max range. Conflicts with
    /// existing enchantments are not checked; use HasConflictingEnchant first.</summary>
    public bool AddEnchant(string id, int level, bool force = false)
        => Bridge.CallBoolStrInt(T->ItemMetaAddEnchant, Ptr, id, level, force);

    /// <summary>Removes the enchantment if present. Returns true if it was removed.</summary>
    public bool RemoveEnchant(string id) => Bridge.CallBoolStr(T->ItemMetaRemoveEnchant, Ptr, id);

    /// <summary>Removes all enchantments.</summary>
    public void RemoveEnchants() => T->ItemMetaRemoveEnchants(Ptr);

    /// <summary>Whether any enchantment conflicts with the given one.</summary>
    public bool HasConflictingEnchant(string id)
        => Bridge.CallBoolStr(T->ItemMetaHasConflictingEnchant, Ptr, id);

    public bool IsUnbreakable
    {
        get => T->ItemMetaIsUnbreakable(Ptr);
        set => T->ItemMetaSetUnbreakable(Ptr, value);
    }

    public bool HasDamage => T->ItemMetaHasDamage(Ptr);

    /// <summary>The damage value, or 0 when unset.</summary>
    public int Damage
    {
        get => T->ItemMetaGetDamage(Ptr);
        set => T->ItemMetaSetDamage(Ptr, value);
    }

    public bool HasRepairCost => T->ItemMetaHasRepairCost(Ptr);

    /// <summary>The repair penalty, or 0 when unset.</summary>
    public int RepairCost
    {
        get => T->ItemMetaGetRepairCost(Ptr);
        set => T->ItemMetaSetRepairCost(Ptr, value);
    }

    /// <summary>Creates a copy of this meta.</summary>
    public ItemMeta Clone() => Wrap((IntPtr)T->ItemMetaClone(Ptr));

    public void Dispose()
    {
        if (_ptr != null)
        {
            T->ItemMetaDelete(_ptr);
            _ptr = null;
        }
        GC.SuppressFinalize(this);
    }

    ~ItemMeta() => Dispose();
}

/// <summary>Wraps endstone::WritableBookMeta (book and quill).</summary>
public unsafe class WritableBookMeta : ItemMeta
{
    internal WritableBookMeta(IntPtr ptr) : base(ptr) { }

    public bool HasPages => T->ItemMetaHasPages(Ptr);
    public int PageCount => T->ItemMetaGetPageCount(Ptr);

    /// <summary>Gets a page (1-based), mirroring WritableBookMeta::getPage.</summary>
    public string GetPage(int page)
    {
        var value = T->ItemMetaGetPage(Ptr, page);
        return value == null ? throw new ArgumentOutOfRangeException(nameof(page)) : Bridge.Str(value);
    }

    /// <summary>Sets a page (1-based). The page must already exist.</summary>
    public void SetPage(int page, string data) => Bridge.CallVoidIntStr(T->ItemMetaSetPage, Ptr, page, data);

    /// <summary>All pages. Setting null or an empty list clears them.</summary>
    public IReadOnlyList<string> Pages
    {
        get
        {
            var count = PageCount;
            var pages = new string[count];
            for (var i = 0; i < count; i++)
            {
                pages[i] = Bridge.Str(T->ItemMetaGetPage(Ptr, i + 1));
            }
            return pages;
        }
        set => Bridge.CallVoidStrArray(T->ItemMetaSetPages, Ptr, value);
    }

    /// <summary>Appends pages to the book.</summary>
    public void AddPages(params string[] pages)
        => Bridge.CallVoidStrArray(T->ItemMetaAddPages, Ptr, pages);
}

/// <summary>Wraps endstone::BookMeta (written book).</summary>
public sealed unsafe class BookMeta : WritableBookMeta
{
    internal BookMeta(IntPtr ptr) : base(ptr) { }

    public bool HasTitle => T->ItemMetaHasTitle(Ptr);

    /// <summary>The book title, or null when unset. Setting null clears it.</summary>
    public string? Title
    {
        get => HasTitle ? Bridge.Str(T->ItemMetaGetTitle(Ptr)) : null;
        set => Bridge.CallVoidStrOrNull(T->ItemMetaSetTitle, Ptr, value);
    }

    public bool HasAuthor => T->ItemMetaHasAuthor(Ptr);

    /// <summary>The book author, or null when unset. Setting null clears it.</summary>
    public string? Author
    {
        get => HasAuthor ? Bridge.Str(T->ItemMetaGetAuthor(Ptr)) : null;
        set => Bridge.CallVoidStrOrNull(T->ItemMetaSetAuthor, Ptr, value);
    }

    public bool HasGeneration => T->ItemMetaHasGeneration(Ptr);

    /// <summary>The copy generation, or null when unset. Setting null clears it.</summary>
    public BookGeneration? Generation
    {
        get
        {
            var generation = T->ItemMetaGetGeneration(Ptr);
            return generation < 0 ? null : (BookGeneration)generation;
        }
        set => T->ItemMetaSetGeneration(Ptr, value.HasValue ? (int)value.Value : -1);
    }
}

/// <summary>Wraps endstone::CrossbowMeta.</summary>
public sealed unsafe class CrossbowMeta : ItemMeta
{
    internal CrossbowMeta(IntPtr ptr) : base(ptr) { }

    public bool HasChargedProjectiles => T->ItemMetaHasChargedProjectiles(Ptr);

    /// <summary>The charged projectiles. Each returned stack owns its native copy.</summary>
    public IReadOnlyList<ItemStack> ChargedProjectiles
    {
        get
        {
            var count = T->ItemMetaGetChargedProjectileCount(Ptr);
            var projectiles = new ItemStack[count];
            for (var i = 0; i < count; i++)
            {
                var stack = T->ItemMetaGetChargedProjectile(Ptr, i);
                projectiles[i] = stack == null
                    ? throw new InvalidOperationException("Failed to read a charged projectile.")
                    : new ItemStack((IntPtr)stack, ownsNative: true);
            }
            return projectiles;
        }
    }

    public void AddChargedProjectile(ItemStack item)
        => T->ItemMetaAddChargedProjectile(Ptr, (void*)item.NativePtr);

    /// <summary>Replaces the charged projectiles.</summary>
    public void SetChargedProjectiles(IEnumerable<ItemStack> items)
    {
        var stacks = items.ToArray();
        if (stacks.Length == 0)
        {
            T->ItemMetaSetChargedProjectiles(Ptr, null, 0);
            return;
        }
        var pointers = new void*[stacks.Length];
        for (var i = 0; i < stacks.Length; i++)
        {
            pointers[i] = (void*)stacks[i].NativePtr;
        }
        fixed (void** p = pointers)
        {
            T->ItemMetaSetChargedProjectiles(Ptr, p, pointers.Length);
        }
    }

    public void ClearChargedProjectiles() => T->ItemMetaClearChargedProjectiles(Ptr);
}

/// <summary>Wraps endstone::MapMeta.</summary>
public sealed unsafe class MapMeta : ItemMeta
{
    internal MapMeta(IntPtr ptr) : base(ptr) { }

    public bool HasMapId => T->ItemMetaHasMapId(Ptr);

    public long MapId
    {
        get => T->ItemMetaGetMapId(Ptr);
        set => T->ItemMetaSetMapId(Ptr, value);
    }

    public bool HasMapView => T->ItemMetaHasMapView(Ptr);

    /// <summary>The associated map view, or null when unset. Setting null clears it.</summary>
    public MapView? View
    {
        get
        {
            var view = T->ItemMetaGetMapView(Ptr);
            return view == null ? null : new MapView((IntPtr)view);
        }
        set => T->ItemMetaSetMapView(Ptr, value == null ? null : (void*)value.NativePtr);
    }
}
