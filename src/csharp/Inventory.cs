namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Inventory (server-owned, never deleted).
/// Item snapshots returned by getters are transient copies — use them immediately.</summary>
public unsafe class Inventory
{
    private readonly void* _ptr;

    internal Inventory(IntPtr ptr) => _ptr = (void*)ptr;
    internal void* NativePtr => _ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public int Size => T->InventoryGetSize(_ptr);
    public int MaxStackSize => T->InventoryGetMaxStackSize(_ptr);
    public bool IsEmpty => T->InventoryIsEmpty(_ptr);

    public ItemStack? GetItem(int index)
    {
        var p = T->InventoryGetItem(_ptr, index);
        return p == null ? null : new ItemStack((IntPtr)p);
    }

    public void SetItem(int index, ItemStack? item)
        => T->InventorySetItem(_ptr, index, item == null ? null : (void*)item.NativePtr);

    public void Clear() => T->InventoryClear(_ptr);
    public int FirstEmpty() => T->InventoryFirstEmpty(_ptr);
    public int First(string type)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(type + "\0");
        fixed (byte* p = buf)
        {
            return T->InventoryFirst(_ptr, p);
        }
    }
    public bool Contains(ItemStack item) => T->InventoryContains(_ptr, (void*)item.NativePtr);
    public bool AddItem(ItemStack item) => T->InventoryAddItem(_ptr, (void*)item.NativePtr);
    public bool RemoveItem(ItemStack item) => T->InventoryRemoveItem(_ptr, (void*)item.NativePtr);
}