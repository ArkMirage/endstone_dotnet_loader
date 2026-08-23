namespace Endstone.Loader;

/// <summary>Locations for displaying objectives to players.</summary>
public enum DisplaySlot
{
    /// <summary>Displays the score below the player's name.</summary>
    BelowName = 0,
    /// <summary>Displays the score in the player list on the pause screen.</summary>
    PlayerList = 1,
    /// <summary>Displays the score on the side of the player's screen.</summary>
    SideBar = 2,
}

/// <summary>Sort order of objectives within a display slot.</summary>
public enum ObjectiveSortOrder
{
    Ascending = 0,
    Descending = 1,
}

/// <summary>Controls how an objective is rendered on the client side.</summary>
public enum RenderType
{
    /// <summary>Display the integer value.</summary>
    Integer = 0,
    /// <summary>Display hearts corresponding to the value.</summary>
    Hearts = 1,
}

/// <summary>Criteria types usable when registering an objective.</summary>
public enum CriteriaType
{
    /// <summary>The dummy criteria; not changed by the server.</summary>
    Dummy = 0,
}

/// <summary>An entry tracked by a scoreboard: a player, an actor, or a fake
/// player name. Implicitly convertible from <see cref="Player"/>,
/// <see cref="Actor"/> and <see cref="string"/>.</summary>
public readonly struct ScoreEntry
{
    // Mirrors the native variant alternative index (see bridge.h).
    internal int Kind { get; }

    public Player? Player { get; }
    public Actor? Actor { get; }
    public string Name { get; }

    private ScoreEntry(int kind, Player? player, Actor? actor, string name)
    {
        Kind = kind;
        Player = player;
        Actor = actor;
        Name = name;
    }

    internal static ScoreEntry ForPlayer(Player player) => new(0, player, null, string.Empty);

    internal static ScoreEntry ForActor(Actor actor) => new(1, null, actor, string.Empty);

    internal static ScoreEntry ForName(string name) => new(2, null, null, name);

    public static implicit operator ScoreEntry(Player player) => ForPlayer(player);

    public static implicit operator ScoreEntry(Actor actor) => ForActor(actor);

    public static implicit operator ScoreEntry(string name) => ForName(name);

    public override string ToString() => Kind switch
    {
        0 => $"Player:{Player?.Name}",
        1 => $"Actor:{Actor?.Id}",
        _ => $"FakePlayer:{Name}",
    };
}

/// <summary>A scoreboard criteria (read-only snapshot taken from an objective).</summary>
public sealed class Criteria
{
    internal Criteria(string name, bool isReadOnly, RenderType defaultRenderType)
    {
        Name = name;
        IsReadOnly = isReadOnly;
        DefaultRenderType = defaultRenderType;
    }

    /// <summary>Unique id of this criteria (e.g. "dummy").</summary>
    public string Name { get; }

    /// <summary>Whether scores under this criteria cannot be modified by plugins.</summary>
    public bool IsReadOnly { get; }

    /// <summary>Render type used by default for this criteria.</summary>
    public RenderType DefaultRenderType { get; }
}

/// <summary>
/// Handle to an objective on a scoreboard. The scoreboard module is primarily
/// a client-side display system, so this wrapper is a lightweight proxy over
/// the objective that the scoreboard itself owns; it only manages the small
/// native proxy object allocated per bridge call.
///
/// Cleanup is automatic via finalizer (the native teardown is a pure memory
/// operation, safe off the server thread) -- dropping the wrapper or an array
/// of wrappers is fine and leaks nothing. No explicit Dispose needed.
/// </summary>
public sealed unsafe class Objective : IEquatable<Objective>
{
    private static Bridge.Table* T => Bridge.Raw;

    private void* _ptr;

    internal Objective(IntPtr ptr) => _ptr = (void*)ptr;

    internal IntPtr NativePtr => (IntPtr)_ptr;

    ~Objective()
    {
        if (_ptr != null)
        {
            T->ObjectiveDelete(_ptr);
            _ptr = null;
        }
    }

    /// <summary>Name of this objective (its unique id on the scoreboard).</summary>
    public string Name => Bridge.Str(T->ObjectiveGetName(_ptr));

    /// <summary>Name displayed to players for this objective.</summary>
    public string DisplayName
    {
        get => Bridge.Str(T->ObjectiveGetDisplayName(_ptr));
        set => Bridge.Call1(T->ObjectiveSetDisplayName, _ptr, value);
    }

    /// <summary>Criteria this objective tracks.</summary>
    public Criteria Criteria => new(
        Bridge.Str(T->ObjectiveGetCriteriaName(_ptr)),
        T->ObjectiveIsCriteriaReadOnly(_ptr),
        (RenderType)T->ObjectiveGetCriteriaRenderType(_ptr));

    /// <summary>Whether the scores of this objective can be modified directly by a plugin.</summary>
    public bool IsModifiable => T->ObjectiveIsModifiable(_ptr);

    /// <summary>The scoreboard this objective is attached to (non-owning view).</summary>
    public Scoreboard Scoreboard => new(T->ObjectiveGetScoreboard(_ptr));

    /// <summary>Whether this objective is currently displayed in any slot.</summary>
    public bool IsDisplayed => T->ObjectiveIsDisplayed(_ptr);

    /// <summary>The slot this objective is displayed at, or null when not displayed.</summary>
    public DisplaySlot? DisplaySlot
    {
        get
        {
            var slot = T->ObjectiveGetDisplaySlot(_ptr);
            return slot < 0 ? null : (DisplaySlot)slot;
        }
        set => T->ObjectiveSetDisplaySlot(_ptr, value.HasValue ? (int)value.Value : -1);
    }

    /// <summary>The sort order of this objective in its display slot, or null when not displayed.</summary>
    public ObjectiveSortOrder? SortOrder
    {
        get
        {
            var order = T->ObjectiveGetSortOrder(_ptr);
            return order < 0 ? null : (ObjectiveSortOrder)order;
        }
    }

    /// <summary>Manner in which this objective is rendered.</summary>
    public RenderType RenderType => (RenderType)T->ObjectiveGetRenderType(_ptr);

    /// <summary>Unregisters this objective from its scoreboard. The native
    /// objective becomes invalid afterwards; discard this wrapper afterwards.</summary>
    public void Unregister() => T->ObjectiveUnregister(_ptr);

    /// <summary>Sets the sort order for this objective in its current display slot.</summary>
    public void SetSortOrder(ObjectiveSortOrder order) => T->ObjectiveSetSortOrder(_ptr, (int)order);

    /// <summary>Sets the display slot and sort order for this objective
    /// (removes it from any other slot). A null slot clears all displays.</summary>
    public void SetDisplay(DisplaySlot? slot, ObjectiveSortOrder order = ObjectiveSortOrder.Ascending)
        => T->ObjectiveSetDisplay(_ptr, slot.HasValue ? (int)slot.Value : -1, (int)order);

    /// <summary>Gets (or creates) the score tracking the given entry on this objective.</summary>
    public Score GetScore(ScoreEntry entry)
    {
        var score = Bridge.CallScoreEntry(T->ObjectiveGetScore, _ptr, entry);
        return score == null ? throw new InvalidOperationException("Failed to get score") : new Score(score);
    }

    public bool Equals(Objective? other)
    {
        if (other is null || _ptr == null || other._ptr == null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return T->ObjectiveEquals(_ptr, other._ptr);
    }

    public override bool Equals(object? obj) => obj is Objective other && Equals(other);

    // Objectives with equal names compare equal through the bridge; names are
    // unique per scoreboard which makes this a stable hash for plugin use.
    public override int GetHashCode() => Name.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(Objective? left, Objective? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Objective? left, Objective? right) => !(left == right);
}

/// <summary>
/// Handle to a score entry on an objective. Like <see cref="Objective"/>,
/// this is a lightweight display-system proxy: cleanup is automatic via
/// finalizer, so query results (e.g. from Scoreboard.GetScores) can simply be
/// dropped without any explicit disposal.
/// </summary>
public sealed unsafe class Score
{
    private static Bridge.Table* T => Bridge.Raw;

    private void* _ptr;

    internal Score(void* ptr) => _ptr = ptr;

    ~Score()
    {
        if (_ptr != null)
        {
            T->ScoreDelete(_ptr);
            _ptr = null;
        }
    }

    /// <summary>The entry being tracked by this score.</summary>
    public ScoreEntry Entry => Bridge.ReadScoreEntry(T->ScoreGetEntry, _ptr);

    /// <summary>The current score value.</summary>
    public int Value
    {
        get => T->ScoreGetValue(_ptr);
        set => T->ScoreSetValue(_ptr, value);
    }

    /// <summary>Whether this score has been set at any point in time.</summary>
    public bool IsScoreSet => T->ScoreIsScoreSet(_ptr);

    /// <summary>The objective being tracked (fresh proxy handle).</summary>
    public Objective Objective => new((IntPtr)T->ScoreGetObjective(_ptr));

    /// <summary>The owning objective's scoreboard (non-owning view).</summary>
    public Scoreboard Scoreboard => new(T->ScoreGetScoreboard(_ptr));
}

/// <summary>
/// Non-owning view of a scoreboard (the main server scoreboard or the one
/// currently assigned to a player). Not disposable: the native object is owned
/// by the server. Use <see cref="Server.CreateScoreboard"/> to obtain an
/// <see cref="OwnedScoreboard"/> when the plugin needs its own scoreboard.
///
/// Objective/Score handles clean up automatically via finalizers; only
/// OwnedScoreboard requires explicit disposal (a game-state operation).
/// </summary>
public unsafe class Scoreboard
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal Scoreboard(IntPtr ptr) => _ptr = (void*)ptr;

    internal Scoreboard(void* ptr) => _ptr = ptr;

    internal IntPtr NativePtr => (IntPtr)_ptr;

    /// <summary>Registers a new objective on this scoreboard.</summary>
    public Objective AddObjective(string name, CriteriaType criteria = CriteriaType.Dummy,
                                  string? displayName = null, RenderType renderType = RenderType.Integer)
    {
        var nameBuf = Bridge.ToUtf8(name);
        var displayBuf = displayName == null ? null : Bridge.ToUtf8(displayName);
        fixed (byte* pn = nameBuf)
        fixed (byte* pd = displayBuf)
        {
            var objective = T->ScoreboardAddObjective(_ptr, pn, (int)criteria, pd, (int)renderType);
            return objective == null
                ? throw new InvalidOperationException($"Objective '{name}' could not be registered (duplicate name?).")
                : new Objective((IntPtr)objective);
        }
    }

    /// <summary>Gets the objective with the given name, or null if it does not exist.</summary>
    public Objective? GetObjective(string name)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            var objective = T->ScoreboardGetObjective(_ptr, p);
            return objective == null ? null : new Objective((IntPtr)objective);
        }
    }

    /// <summary>Gets the objective currently displayed in the given slot, or null.</summary>
    public Objective? GetObjective(DisplaySlot slot)
    {
        var objective = T->ScoreboardGetObjectiveInSlot(_ptr, (int)slot);
        return objective == null ? null : new Objective((IntPtr)objective);
    }

    /// <summary>Gets all objectives on this scoreboard. The returned wrappers
    /// clean up after themselves; the array can simply be dropped.</summary>
    public Objective[] Objectives
    {
        get
        {
            const int capacity = 256;
            var buffer = stackalloc void*[capacity];
            var count = T->ScoreboardGetObjectives(_ptr, buffer, capacity);
            var objectives = new Objective[count];
            for (var i = 0; i < count; i++)
            {
                objectives[i] = new Objective((IntPtr)buffer[i]);
            }
            return objectives;
        }
    }

    /// <summary>Gets all objectives using the given criteria. Caller owns the returned wrappers.</summary>
    public Objective[] GetObjectivesByCriteria(CriteriaType criteria)
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var count = T->ScoreboardGetObjectivesByCriteria(_ptr, (int)criteria, buffer, capacity);
        var objectives = new Objective[count];
        for (var i = 0; i < count; i++)
        {
            objectives[i] = new Objective((IntPtr)buffer[i]);
        }
        return objectives;
    }

    /// <summary>Gets all scores tracked for the given entry across every objective.</summary>
    public Score[] GetScores(ScoreEntry entry)
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var count = Bridge.CallScoreEntryScores(T->ScoreboardGetScores, _ptr, entry, buffer, capacity);
        var scores = new Score[count];
        for (var i = 0; i < count; i++)
        {
            scores[i] = new Score(buffer[i]);
        }
        return scores;
    }

    /// <summary>Removes all scores tracked for the given entry on this scoreboard.</summary>
    public void ResetScores(ScoreEntry entry) => Bridge.CallScoreEntryVoid(T->ScoreboardResetScores, _ptr, entry);

    /// <summary>Gets all entries tracked by this scoreboard.</summary>
    public ScoreEntry[] Entries
    {
        get
        {
            var count = T->ScoreboardGetEntryCount(_ptr);
            var entries = new ScoreEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = Bridge.ReadScoreEntryIndexed(T->ScoreboardGetEntry, _ptr, i);
            }
            return entries;
        }
    }

    /// <summary>Clears any objective displayed in the given slot.</summary>
    public void ClearSlot(DisplaySlot slot) => T->ScoreboardClearSlot(_ptr, (int)slot);
}

/// <summary>
/// A scoreboard created by <see cref="Server.CreateScoreboard"/>. The wrapper
/// owns the plugin's native shared_ptr reference; Dispose() destroys it.
/// </summary>
public sealed unsafe class OwnedScoreboard : Scoreboard, IDisposable
{
    private static Bridge.Table* T => Bridge.Raw;

    // Heap std::shared_ptr<endstone::Scoreboard> holder; freed via ScoreboardRelease.
    private void* _holder;

    internal OwnedScoreboard(void* ptr, void* holder) : base(ptr) => _holder = holder;

    /// <summary>Disposes the native scoreboard. The wrapper becomes invalid afterwards. Should be call in main thread</summary>
    public void Dispose()
    {
        if (_holder != null)
        {
            T->ScoreboardRelease(_holder);
            _holder = null;
        }
    }
}
