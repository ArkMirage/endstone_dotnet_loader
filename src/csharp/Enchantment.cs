namespace Endstone.Loader;

/// <summary>An enchantment applied to an item stack, paired with its level.</summary>
public readonly record struct ItemEnchantment(Enchantment Enchantment, int Level);

/// <summary>Wraps a native endstone::Enchantment registry entry. Instances are
/// transient views of server-owned objects; do not store them across plugin
/// reloads.</summary>
public sealed unsafe class Enchantment
{
    private readonly void* _ptr;

    internal Enchantment(IntPtr ptr) => _ptr = (void*)ptr;

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>Full identifier of this enchantment, e.g. "minecraft:sharpness".</summary>
    public string Id => Bridge.Str(T->EnchantGetId(_ptr));

    /// <summary>Namespace part of the identifier, e.g. "minecraft".</summary>
    public string Namespace
    {
        get
        {
            var sep = Id.IndexOf(':');
            return sep < 0 ? Id : Id[..sep];
        }
    }

    /// <summary>Key part of the identifier, e.g. "sharpness".</summary>
    public string Key
    {
        get
        {
            var sep = Id.IndexOf(':');
            return sep < 0 ? Id : Id[(sep + 1)..];
        }
    }

    /// <summary>Gets the highest level this enchantment may reach.</summary>
    public int MaxLevel => T->EnchantGetMaxLevel(_ptr);

    /// <summary>Gets the level this enchantment starts at (its minimum level).</summary>
    public int StartLevel => T->EnchantGetStartLevel(_ptr);

    /// <summary>Checks whether this enchantment conflicts with another one.</summary>
    public bool ConflictsWith(Enchantment? other)
        => other != null && T->EnchantConflictsWith(_ptr, (void*)other.NativePtr);

    /// <summary>Checks whether this enchantment may be applied to the given item stack.</summary>
    public bool CanEnchantItem(ItemStack? item)
        => item != null && T->EnchantCanEnchantItem(_ptr, (void*)item.NativePtr);

    /// <summary>Looks up an enchantment by identifier ("minecraft:sharpness", or a
    /// bare "sharpness" which implies the minecraft namespace). Returns null when
    /// no such enchantment is registered.</summary>
    public static Enchantment? Get(string id)
    {
        var buf = Bridge.ToUtf8(id);
        fixed (byte* p = buf)
        {
            var e = T->EnchantGetById(p);
            return e == null ? null : new Enchantment((IntPtr)e);
        }
    }

    public override string ToString() => Id;

    // ---- built-in enchantments ----
    public static Enchantment? Protection => Get("minecraft:protection");
    public static Enchantment? FireProtection => Get("minecraft:fire_protection");
    public static Enchantment? FeatherFalling => Get("minecraft:feather_falling");
    public static Enchantment? BlastProtection => Get("minecraft:blast_protection");
    public static Enchantment? ProjectileProtection => Get("minecraft:projectile_protection");
    public static Enchantment? Thorns => Get("minecraft:thorns");
    public static Enchantment? Respiration => Get("minecraft:respiration");
    public static Enchantment? DepthStrider => Get("minecraft:depth_strider");
    public static Enchantment? AquaAffinity => Get("minecraft:aqua_affinity");
    public static Enchantment? Sharpness => Get("minecraft:sharpness");
    public static Enchantment? Smite => Get("minecraft:smite");
    public static Enchantment? BaneOfArthropods => Get("minecraft:bane_of_arthropods");
    public static Enchantment? Knockback => Get("minecraft:knockback");
    public static Enchantment? FireAspect => Get("minecraft:fire_aspect");
    public static Enchantment? Looting => Get("minecraft:looting");
    public static Enchantment? Efficiency => Get("minecraft:efficiency");
    public static Enchantment? SilkTouch => Get("minecraft:silk_touch");
    public static Enchantment? Unbreaking => Get("minecraft:unbreaking");
    public static Enchantment? Fortune => Get("minecraft:fortune");
    public static Enchantment? Power => Get("minecraft:power");
    public static Enchantment? Punch => Get("minecraft:punch");
    public static Enchantment? Flame => Get("minecraft:flame");
    public static Enchantment? Infinity => Get("minecraft:infinity");
    public static Enchantment? LuckOfTheSea => Get("minecraft:luck_of_the_sea");
    public static Enchantment? Lure => Get("minecraft:lure");
    public static Enchantment? FrostWalker => Get("minecraft:frost_walker");
    public static Enchantment? Mending => Get("minecraft:mending");
    public static Enchantment? CurseOfBinding => Get("minecraft:binding");
    public static Enchantment? CurseOfVanishing => Get("minecraft:vanishing");
    public static Enchantment? Impaling => Get("minecraft:impaling");
    public static Enchantment? Riptide => Get("minecraft:riptide");
    public static Enchantment? Loyalty => Get("minecraft:loyalty");
    public static Enchantment? Channeling => Get("minecraft:channeling");
    public static Enchantment? Multishot => Get("minecraft:multishot");
    public static Enchantment? Piercing => Get("minecraft:piercing");
    public static Enchantment? QuickCharge => Get("minecraft:quick_charge");
    public static Enchantment? SoulSpeed => Get("minecraft:soul_speed");
    public static Enchantment? SwiftSneak => Get("minecraft:swift_sneak");
    public static Enchantment? WindBurst => Get("minecraft:wind_burst");
    public static Enchantment? Density => Get("minecraft:density");
    public static Enchantment? Breach => Get("minecraft:breach");
    public static Enchantment? Lunge => Get("minecraft:lunge");
}