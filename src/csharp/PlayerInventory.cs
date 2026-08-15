namespace Endstone.Loader;

/// <summary>Wraps a native endstone::PlayerInventory (player's 36 slots + armor + hands).</summary>
public sealed unsafe class PlayerInventory : Inventory
{
    internal PlayerInventory(IntPtr ptr) : base(ptr) { }

    private static Bridge.Table* T => Bridge.Raw;

    private ItemStack? Slot(void* p) => p == null ? null : new ItemStack((IntPtr)p);

    public ItemStack? ItemInMainHand => Slot(T->InventoryGetItemInMainHand(NativePtr));
    public ItemStack? ItemInOffHand => Slot(T->InventoryGetItemInOffHand(NativePtr));
    public ItemStack? Helmet => Slot(T->InventoryGetHelmet(NativePtr));
    public ItemStack? Chestplate => Slot(T->InventoryGetChestplate(NativePtr));
    public ItemStack? Leggings => Slot(T->InventoryGetLeggings(NativePtr));
    public ItemStack? Boots => Slot(T->InventoryGetBoots(NativePtr));

    public void SetItemInMainHand(ItemStack? item)
        => T->InventorySetItemInMainHand(NativePtr, item == null ? null : (void*)item.NativePtr);
    public void SetItemInOffHand(ItemStack? item)
        => T->InventorySetItemInOffHand(NativePtr, item == null ? null : (void*)item.NativePtr);
    public void SetHelmet(ItemStack? item)
        => T->InventorySetHelmet(NativePtr, item == null ? null : (void*)item.NativePtr);
    public void SetChestplate(ItemStack? item)
        => T->InventorySetChestplate(NativePtr, item == null ? null : (void*)item.NativePtr);
    public void SetLeggings(ItemStack? item)
        => T->InventorySetLeggings(NativePtr, item == null ? null : (void*)item.NativePtr);
    public void SetBoots(ItemStack? item)
        => T->InventorySetBoots(NativePtr, item == null ? null : (void*)item.NativePtr);

    public int HeldItemSlot
    {
        get => T->InventoryGetHeldItemSlot(NativePtr);
        set => T->InventorySetHeldItemSlot(NativePtr, value);
    }
}