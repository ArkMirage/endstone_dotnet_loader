namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Item, a dropped item entity in the level.</summary>
public sealed unsafe class Item : Actor
{
    internal Item(IntPtr ptr) : base(ptr) { }

    private static Bridge.Table* T => Bridge.Raw;
    private void* P => (void*)NativePtr;

    /// <summary>Gets or sets the item stack carried by this dropped item.
    /// The getter returns a transient snapshot; read it before the next item
    /// call on the same thread.</summary>
    public ItemStack ItemStack
    {
        get => new((IntPtr)T->ItemActorGetItemStack(P));
        set => T->ItemActorSetItemStack(P, (void*)value.NativePtr);
    }

    /// <summary>Delay in ticks before players can pick this item up.</summary>
    public int PickupDelay
    {
        get => T->ItemActorGetPickupDelay(P);
        set => T->ItemActorSetPickupDelay(P, value);
    }

    /// <summary>Whether this item never despawns.</summary>
    public bool UnlimitedLifetime
    {
        get => T->ItemActorIsUnlimitedLifetime(P);
        set => T->ItemActorSetUnlimitedLifetime(P, value);
    }

    /// <summary>Entity ID of the player who dropped this item, or null when
    /// the thrower is unknown.</summary>
    public long? Thrower
    {
        get
        {
            long value;
            return T->ItemActorGetThrower(P, &value) ? value : null;
        }
        set => T->ItemActorSetThrower(P, value.HasValue, value.GetValueOrDefault());
    }
}
