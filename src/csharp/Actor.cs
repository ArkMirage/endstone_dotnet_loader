namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Actor.</summary>
public unsafe class Actor : CommandSender
{
    internal Actor(IntPtr ptr) : base(ptr) { }

    private static Bridge.Table* T => Bridge.Raw;

    public string Type => Bridge.Str(T->ActorGetType(_ptr));
    public ulong RuntimeId => T->ActorGetRuntimeId(_ptr);
    public long Id => T->ActorGetId(_ptr);
    public bool IsOnGround => T->ActorIsOnGround(_ptr);
    public bool IsInWater => T->ActorIsInWater(_ptr);
    public bool IsInLava => T->ActorIsInLava(_ptr);
    public bool IsDead => T->ActorIsDead(_ptr);
    public bool IsValid => T->ActorIsValid(_ptr);
    public string DimensionName => Bridge.Str(T->ActorGetDimensionName(_ptr));
    public string NameTag => Bridge.Str(T->ActorGetNameTag(_ptr));
    public string ScoreTag => Bridge.Str(T->ActorGetScoreTag(_ptr));
    public bool IsNameTagVisible => T->ActorIsNameTagVisible(_ptr);
    public bool IsNameTagAlwaysVisible => T->ActorIsNameTagAlwaysVisible(_ptr);
    public int ScoreboardTagCount => T->ActorGetScoreboardTagCount(_ptr);

    public Location Location
    {
        get
        {
            var values = stackalloc float[5];
            T->ActorGetLocation(_ptr, values);
            return new Location(values[0], values[1], values[2], values[3], values[4]);
        }
    }

    public Location Velocity
    {
        get
        {
            var values = stackalloc float[3];
            T->ActorGetVelocity(_ptr, values);
            return new Location(values[0], values[1], values[2]);
        }
    }

    public void SetRotation(float yaw, float pitch) => T->ActorSetRotation(_ptr, yaw, pitch);

    /// <summary>Teleports this actor to the given location (same dimension).</summary>
    public bool Teleport(Location location)
    {
        var values = stackalloc float[5] { location.X, location.Y, location.Z, location.Pitch, location.Yaw };
        return T->ActorTeleportLocation(_ptr, values);
    }

    /// <summary>Teleports this actor to another actor's position.</summary>
    public bool Teleport(Actor target) => T->ActorTeleportActor(_ptr, (void*)target.NativePtr);

    /// <summary>Removes this actor from the level (use Player.Kick for players).</summary>
    public void Remove() => T->ActorRemove(_ptr);

    public string GetScoreboardTag(int index) => Bridge.Str(T->ActorGetScoreboardTag(_ptr, index));

    public bool AddScoreboardTag(string tag) => Bridge.CallBoolStr(T->ActorAddScoreboardTag, _ptr, tag);

    public bool RemoveScoreboardTag(string tag) => Bridge.CallBoolStr(T->ActorRemoveScoreboardTag, _ptr, tag);

    public void SetNameTagVisible(bool visible) => T->ActorSetNameTagVisible(_ptr, visible);

    public void SetNameTagAlwaysVisible(bool visible) => T->ActorSetNameTagAlwaysVisible(_ptr, visible);

    public void SetNameTag(string nameTag) => Bridge.Call1(T->ActorSetNameTag, _ptr, nameTag);

    public void SetScoreTag(string scoreTag) => Bridge.Call1(T->ActorSetScoreTag, _ptr, scoreTag);

    /// <summary>Downcasts this actor to Mob, or null if it is not a mob.</summary>
    public Mob? AsMob()
    {
        var m = T->ActorAsMob(_ptr);
        return m == null ? null : new Mob((IntPtr)m);
    }

    /// <summary>Spawns an actor of the given type (e.g. "minecraft:zombie") at a
    /// location in this actor's dimension. Returns null if spawning failed.</summary>
    public Actor? SpawnActor(string type, Location? location = null)
    {
        var loc = location ?? Location;
        var values = stackalloc float[5] { loc.X, loc.Y, loc.Z, loc.Pitch, loc.Yaw };
        var buf = System.Text.Encoding.UTF8.GetBytes(type + "\0");
        fixed (byte* p = buf)
        {
            var a = T->ActorSpawnActor(_ptr, values, p);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }

    /// <summary>Spawns a mob and returns it, or null if it is not a mob / failed to spawn.</summary>
    public Mob? SpawnMob(string type, Location? location = null) => SpawnActor(type, location)?.AsMob();

    /// <summary>Gets the block at the given block coordinates in this actor's dimension.</summary>
    public Block? GetBlock(int x, int y, int z)
    {
        var b = T->DimensionGetBlockAt(T->ActorGetDimension(_ptr), x, y, z);
        return b == null ? null : new Block((IntPtr)b, ownsPtr: true);
    }
}