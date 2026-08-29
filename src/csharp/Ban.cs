namespace Endstone.Loader;

/// <summary>
/// Non-owning view of a ban entry. Properties read from and write to the
/// native BanEntry via the bridge, supporting the get → filter → set pattern.
/// The entry's lifetime is managed by the owning <see cref="PlayerBanList"/>
/// or <see cref="IpBanList"/>.
/// </summary>
public unsafe class BanEntry
{
    private static Bridge.Table* T => Bridge.Raw;

    internal readonly void* _ptr;

    internal BanEntry(void* ptr) => _ptr = ptr;

    /// <summary>Date and time when this ban entry was created.</summary>
    public DateTimeOffset Created
    {
        get => DateTimeOffset.FromUnixTimeSeconds(T->BanEntryGetCreated(_ptr));
        set => T->BanEntrySetCreated(_ptr, value.ToUnixTimeSeconds());
    }

    /// <summary>The source of the ban (e.g. the operator name).</summary>
    public string Source
    {
        get => Bridge.Str(T->BanEntryGetSource(_ptr));
        set => Bridge.Call1(T->BanEntrySetSource, _ptr, value);
    }

    /// <summary>The date and time at which this ban expires,
    /// or null for a permanent ban.</summary>
    public DateTimeOffset? Expiration
    {
        get
        {
            var ts = T->BanEntryGetExpiration(_ptr);
            return ts < 0 ? null : DateTimeOffset.FromUnixTimeSeconds(ts);
        }
        set => T->BanEntrySetExpiration(_ptr, value.HasValue ? value.Value.ToUnixTimeSeconds() : -1);
    }

    /// <summary>The reason for the ban.</summary>
    public string Reason
    {
        get => Bridge.Str(T->BanEntryGetReason(_ptr));
        set => Bridge.Call1(T->BanEntrySetReason, _ptr, value);
    }
}

/// <summary>
/// Non-owning view of a player ban entry. Extends <see cref="BanEntry"/>
/// with player-specific read-only fields (Name, UniqueId, Xuid).
/// </summary>
public sealed unsafe class PlayerBanEntry : BanEntry
{
    private static Bridge.Table* T => Bridge.Raw;

    internal PlayerBanEntry(void* ptr) : base(ptr) { }

    /// <summary>The name of the banned player.</summary>
    public string Name => Bridge.Str(T->PlayerBanEntryGetName(_ptr));

    /// <summary>The unique ID of the banned player, or null if not available.</summary>
    public Guid? UniqueId
    {
        get
        {
            byte* buf = stackalloc byte[16];
            return T->PlayerBanEntryGetUuid(_ptr, buf) ? UuidMarshal.Read(new ReadOnlySpan<byte>(buf, 16)) : null;
        }
    }

    /// <summary>The XUID of the banned player, or null if not available.</summary>
    public string? Xuid
    {
        get
        {
            var p = T->PlayerBanEntryGetXuid(_ptr);
            return p == null ? null : Bridge.Str(p);
        }
    }
}

/// <summary>
/// Non-owning view of an IP ban entry. Extends <see cref="BanEntry"/>
/// with the banned address.
/// </summary>
public sealed unsafe class IpBanEntry : BanEntry
{
    private static Bridge.Table* T => Bridge.Raw;

    internal IpBanEntry(void* ptr) : base(ptr) { }

    /// <summary>The banned IP address.</summary>
    public string Address => Bridge.Str(T->IpBanEntryGetAddress(_ptr));
}

/// <summary>
/// Non-owning view of the player ban list. No disposal required —
/// the server owns the underlying object.
/// </summary>
public sealed unsafe class PlayerBanList
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal PlayerBanList(void* ptr) => _ptr = ptr;

    /// <summary>
    /// Gets the ban entry for the given player name, or null if not banned.
    /// </summary>
    public PlayerBanEntry? GetBanEntry(string name)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            var entry = T->PlayerBanListGetBanEntry(_ptr, p);
            return entry == null ? null : new PlayerBanEntry(entry);
        }
    }

    /// <summary>
    /// Adds a ban to the list with an explicit expiration time.
    /// </summary>
    /// <param name="name">The player name to ban.</param>
    /// <param name="reason">The reason for the ban, or null for default.</param>
    /// <param name="expires">When the ban expires, or null for a permanent ban.</param>
    /// <param name="source">The source of the ban, or null for default.</param>
    /// <returns>The created ban entry.</returns>
    public PlayerBanEntry AddBan(string name, string? reason = null, DateTimeOffset? expires = null, string? source = null)
    {
        var nameBuf = Bridge.ToUtf8(name);
        var reasonBuf = reason == null ? null : Bridge.ToUtf8(reason);
        var sourceBuf = source == null ? null : Bridge.ToUtf8(source);
        fixed (byte* pn = nameBuf)
        fixed (byte* pr = reasonBuf)
        fixed (byte* ps = sourceBuf)
        {
            var entry = T->PlayerBanListAddBan(_ptr, pn, pr, expires.HasValue ? expires.Value.ToUnixTimeSeconds() : -1, ps);
            return new PlayerBanEntry(entry);
        }
    }

    /// <summary>
    /// Adds a ban to the list with a duration from now.
    /// </summary>
    /// <param name="name">The player name to ban.</param>
    /// <param name="reason">The reason for the ban, or null for default.</param>
    /// <param name="duration">How long the ban lasts.</param>
    /// <param name="source">The source of the ban, or null for default.</param>
    /// <returns>The created ban entry.</returns>
    public PlayerBanEntry AddBan(string name, string? reason, TimeSpan duration, string? source = null)
    {
        var expires = DateTimeOffset.UtcNow + duration;
        return AddBan(name, reason, expires, source);
    }

    /// <summary>
    /// Gets all ban entries in this list.
    /// </summary>
    public IReadOnlyList<PlayerBanEntry> GetEntries()
    {
        const int capacity = 512;
        var buffer = stackalloc void*[capacity];
        var total = T->PlayerBanListGetEntries(_ptr, buffer, capacity);
        var count = Math.Min(total, capacity);
        var result = new PlayerBanEntry[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = new PlayerBanEntry(buffer[i]);
        }
        return result;
    }

    /// <summary>
    /// Returns whether the given player is banned.
    /// </summary>
    public bool IsBanned(string name)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            return T->PlayerBanListIsBanned(_ptr, p);
        }
    }

    /// <summary>
    /// Removes the ban for the given player.
    /// </summary>
    public void RemoveBan(string name)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            T->PlayerBanListRemoveBan(_ptr, p);
        }
    }
}

/// <summary>
/// A ban list for IP addresses. Non-owning view of the server-owned list; no
/// disposal required.
/// </summary>
public sealed unsafe class IpBanList
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal IpBanList(void* ptr) => _ptr = ptr;

    /// <summary>
    /// Gets the ban entry for the given address, or null if not banned.
    /// </summary>
    public IpBanEntry? GetBanEntry(string address)
    {
        var buf = Bridge.ToUtf8(address);
        fixed (byte* p = buf)
        {
            var entry = T->IpBanListGetBanEntry(_ptr, p);
            return entry == null ? null : new IpBanEntry(entry);
        }
    }

    /// <summary>
    /// Adds a ban to the list with an explicit expiration time.
    /// </summary>
    /// <param name="address">The IP address to ban.</param>
    /// <param name="reason">The reason for the ban, or null for default.</param>
    /// <param name="expires">When the ban expires, or null for a permanent ban.</param>
    /// <param name="source">The source of the ban, or null for default.</param>
    /// <returns>The created ban entry.</returns>
    public IpBanEntry AddBan(string address, string? reason = null, DateTimeOffset? expires = null, string? source = null)
    {
        var addrBuf = Bridge.ToUtf8(address);
        var reasonBuf = reason == null ? null : Bridge.ToUtf8(reason);
        var sourceBuf = source == null ? null : Bridge.ToUtf8(source);
        fixed (byte* pa = addrBuf)
        fixed (byte* pr = reasonBuf)
        fixed (byte* ps = sourceBuf)
        {
            var entry = T->IpBanListAddBan(_ptr, pa, pr, expires.HasValue ? expires.Value.ToUnixTimeSeconds() : -1, ps);
            return new IpBanEntry(entry);
        }
    }

    /// <summary>
    /// Adds a ban to the list with a duration from now.
    /// </summary>
    /// <param name="address">The IP address to ban.</param>
    /// <param name="reason">The reason for the ban, or null for default.</param>
    /// <param name="duration">How long the ban lasts.</param>
    /// <param name="source">The source of the ban, or null for default.</param>
    /// <returns>The created ban entry.</returns>
    public IpBanEntry AddBan(string address, string? reason, TimeSpan duration, string? source = null)
    {
        var expires = DateTimeOffset.UtcNow + duration;
        return AddBan(address, reason, expires, source);
    }

    /// <summary>
    /// Gets all ban entries in this list.
    /// </summary>
    public IReadOnlyList<IpBanEntry> GetEntries()
    {
        const int capacity = 512;
        var buffer = stackalloc void*[capacity];
        var total = T->IpBanListGetEntries(_ptr, buffer, capacity);
        var count = Math.Min(total, capacity);
        var result = new IpBanEntry[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = new IpBanEntry(buffer[i]);
        }
        return result;
    }

    /// <summary>
    /// Returns whether the given address is banned.
    /// </summary>
    public bool IsBanned(string address)
    {
        var buf = Bridge.ToUtf8(address);
        fixed (byte* p = buf)
        {
            return T->IpBanListIsBanned(_ptr, p);
        }
    }

    /// <summary>
    /// Removes the ban for the given address.
    /// </summary>
    public void RemoveBan(string address)
    {
        var buf = Bridge.ToUtf8(address);
        fixed (byte* p = buf)
        {
            T->IpBanListRemoveBan(_ptr, p);
        }
    }
}
