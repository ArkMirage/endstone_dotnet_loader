using System.Collections.Frozen;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Endstone.Loader;

public enum EventPriority
{
    Lowest = 0,
    Low = 1,
    Normal = 2,
    High = 3,
    Highest = 4,
    Monitor = 5,
}

public enum InteractAction
{
    LeftClickBlock = 0,
    RightClickBlock = 1,
    LeftClickAir = 2,
    RightClickAir = 3,
    Physical = 4,
}

public enum BlockFace
{
    Down,
    Up,
    North,
    South,
    West,
    East,
}

public enum EquipmentSlot
{
    Hand,
    OffHand,
    Feet,
    Legs,
    Chest,
    Head,
    Body,
}

public enum LoadType
{
    Startup,
    Reload,
}

/// <summary>
/// Identifies the concrete event class. The dispatcher resolves the event name
/// to a kind exactly once per event instance; native multi-type accessors
/// switch on this instead of classifying strings (see bridge.h EventKind).
/// Values MUST stay in sync with endstone_dotnet_loader/include/bridge.h.
/// </summary>
internal enum EventKind
{
    PlayerJoinEvent = 0,
    PlayerQuitEvent = 1,
    PlayerLoginEvent = 2,
    PlayerChatEvent = 3,
    PlayerCommandEvent = 4,
    PlayerMoveEvent = 5,
    PlayerTeleportEvent = 6,
    PlayerPortalEvent = 7,
    PlayerDeathEvent = 8,
    PlayerInteractEvent = 9,
    PlayerInteractActorEvent = 10,
    PlayerRespawnEvent = 11,
    PlayerDropItemEvent = 12,
    PlayerGameModeChangeEvent = 13,
    PlayerItemHeldEvent = 14,
    PlayerItemConsumeEvent = 15,
    PlayerKickEvent = 16,
    PlayerPickupItemEvent = 17,
    PlayerJumpEvent = 18,
    PlayerEmoteEvent = 19,
    PlayerBedEnterEvent = 20,
    PlayerBedLeaveEvent = 21,
    PlayerDimensionChangeEvent = 22,
    PlayerSkinChangeEvent = 23,
    ActorDamageEvent = 24,
    ActorDeathEvent = 25,
    ActorExplodeEvent = 26,
    ActorKnockbackEvent = 27,
    ActorTeleportEvent = 28,
    ActorSpawnEvent = 29,
    ActorRemoveEvent = 30,
    BlockBreakEvent = 31,
    BlockPlaceEvent = 32,
    BlockCookEvent = 33,
    BlockExplodeEvent = 34,
    BlockFormEvent = 35,
    BlockGrowEvent = 36,
    BlockFromToEvent = 37,
    BlockPistonExtendEvent = 38,
    BlockPistonRetractEvent = 39,
    LeavesDecayEvent = 40,
    ChunkLoadEvent = 41,
    ChunkUnloadEvent = 42,
    ServerCommandEvent = 43,
    ServerLoadEvent = 44,
    BroadcastMessageEvent = 45,
    ServerListPingEvent = 46,
    PacketReceiveEvent = 47,
    PacketSendEvent = 48,
    PluginEnableEvent = 49,
    PluginDisableEvent = 50,
    ScriptMessageEvent = 51,
    MapInitializeEvent = 52,
    ThunderChangeEvent = 53,
    WeatherChangeEvent = 54,
}

/// <summary>Base class for event wrappers.</summary>
public unsafe abstract class Event
{
    private readonly void* _ptr;
    private EventKind _kind;

    internal Event(IntPtr ptr) => _ptr = (void*)ptr;
    internal void* NativePtr => _ptr;

    /// <summary>Concrete event kind, set by the factory; used by multi-type native accessors.</summary>
    internal EventKind Kind
    {
        set => _kind = value;
    }

    private static Bridge.Table* T => Bridge.Raw;

    public bool IsCancelled
    {
        get => Bridge.CallKindBool(T->EventIsCancelled, _ptr, _kind);
        set => Bridge.CallKind2(T->EventSetCancelled, _ptr, _kind, value);
    }

    public void Cancel() => IsCancelled = true;

    public virtual Player? Player
    {
        get
        {
            var p = Bridge.CallKindPtr(T->EventGetPlayer, _ptr, _kind);
            return p == null ? null : new Player((IntPtr)p);
        }
    }

    public virtual Actor? Actor
    {
        get
        {
            var a = Bridge.CallKindPtr(T->EventGetActor, _ptr, _kind);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }
}

internal static class EventLocationHelper
{
    internal static unsafe Location Read(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr)
    {
        var values = stackalloc float[5];
        fn(ptr, values);
        return new Location(values[0], values[1], values[2], values[3], values[4]);
    }

    internal static unsafe Location Read(delegate* unmanaged[Cdecl]<void*, float*, void> fn,
        delegate* unmanaged[Cdecl]<void*, void*> dimFn, void* ptr)
    {
        var values = stackalloc float[5];
        fn(ptr, values);
        var dim = dimFn(ptr);
        var dimension = dim == null ? null : new Dimension((IntPtr)dim);
        return new Location(values[0], values[1], values[2], values[3], values[4], dimension);
    }

    internal static unsafe void Write(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr, Location loc)
    {
        var values = stackalloc float[5] { loc.X, loc.Y, loc.Z, loc.Pitch, loc.Yaw };
        fn(ptr, values);
    }
}

internal static class EventVectorHelper
{
    internal static unsafe Vector3 Read(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr)
    {
        var values = stackalloc float[3];
        fn(ptr, values);
        return new Vector3(values[0], values[1], values[2]);
    }

    internal static unsafe void Write(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr, Vector3 v)
    {
        var values = stackalloc float[3] { v.X, v.Y, v.Z };
        fn(ptr, values);
    }
}

// ==================== Player events ====================

public sealed unsafe class PlayerJoinEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerJoinEvent(IntPtr ptr) : base(ptr) { }

    public string? JoinMessage
    {
        get
        {
            var p = T->JoinGetMessage(NativePtr);
            return p == null ? null : Bridge.Str(p);
        }
        set => Bridge.Call1(T->JoinSetMessage, NativePtr, value ?? string.Empty);
    }
}

public sealed unsafe class PlayerQuitEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerQuitEvent(IntPtr ptr) : base(ptr) { }

    public string? QuitMessage
    {
        get
        {
            var p = T->QuitGetMessage(NativePtr);
            return p == null ? null : Bridge.Str(p);
        }
        set => Bridge.Call1(T->QuitSetMessage, NativePtr, value ?? string.Empty);
    }
}

public sealed unsafe class PlayerLoginEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerLoginEvent(IntPtr ptr) : base(ptr) { }

    public string KickMessage
    {
        get => Bridge.Str(T->LoginGetKickMessage(NativePtr));
        set => Bridge.Call1(T->LoginSetKickMessage, NativePtr, value);
    }
}

public sealed unsafe class PlayerChatEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerChatEvent(IntPtr ptr) : base(ptr) { }

    public string Message
    {
        get => Bridge.Str(T->ChatGetMessage(NativePtr));
        set => Bridge.Call1(T->ChatSetMessage, NativePtr, value);
    }

    public string Format
    {
        get => Bridge.Str(T->ChatGetFormat(NativePtr));
        set => Bridge.Call1(T->ChatSetFormat, NativePtr, value);
    }

    public int RecipientCount => T->ChatGetRecipientCount(NativePtr);
}

public sealed unsafe class PlayerCommandEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerCommandEvent(IntPtr ptr) : base(ptr) { }

    public string Command
    {
        get => Bridge.Str(T->CommandGetCommand(NativePtr));
        set => Bridge.Call1(T->CommandSetCommand, NativePtr, value);
    }
}

public sealed unsafe class PlayerMoveEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerMoveEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->MoveGetFrom, T->MoveGetFromDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, T->MoveGetToDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetTo, NativePtr, value);
    }
}

public sealed unsafe class PlayerTeleportEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerTeleportEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->MoveGetFrom, T->MoveGetFromDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, T->MoveGetToDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetTo, NativePtr, value);
    }
}

public sealed unsafe class PlayerPortalEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerPortalEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->MoveGetFrom, T->MoveGetFromDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, T->MoveGetToDimension, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetTo, NativePtr, value);
    }
}

public sealed unsafe class PlayerDeathEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerDeathEvent(IntPtr ptr) : base(ptr) { }

    public string? DeathMessage
    {
        get
        {
            var p = T->DeathGetMessage(NativePtr);
            return p == null ? null : Bridge.Str(p);
        }
        set => Bridge.Call1(T->DeathSetMessage, NativePtr, value ?? string.Empty);
    }
}

public sealed unsafe class PlayerInteractEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerInteractEvent(IntPtr ptr) : base(ptr) { }

    public InteractAction Action => (InteractAction)T->InteractGetAction(NativePtr);

    public Vector3? ClickedPosition
    {
        get
        {
            var values = stackalloc float[3];
            if (T->InteractGetClickedPosition(NativePtr, values) == 0)
            {
                return null;
            }
            return new Vector3(values[0], values[1], values[2]);
        }
    }

    public bool HasItem => T->InteractHasItem(NativePtr);

    public ItemStack? Item
    {
        get
        {
            var i = T->InteractGetItem(NativePtr);
            return i == null ? null : new ItemStack((IntPtr)i);
        }
    }

    public bool HasBlock => T->InteractHasBlock(NativePtr);

    public Block? Block
    {
        get
        {
            var b = T->InteractGetBlock(NativePtr);
            return b == null ? null : new Block((IntPtr)b);
        }
    }

    public BlockFace BlockFace => (BlockFace)T->InteractGetBlockFace(NativePtr);
}

public sealed unsafe class PlayerInteractActorEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerInteractActorEvent(IntPtr ptr) : base(ptr) { }

    public override Actor? Actor
    {
        get
        {
            var a = T->InteractActorGetActor(NativePtr);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }
}

public sealed unsafe class PlayerRespawnEvent : Event
{
    internal PlayerRespawnEvent(IntPtr ptr) : base(ptr) { }
}

public sealed unsafe class PlayerDropItemEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerDropItemEvent(IntPtr ptr) : base(ptr) { }

    public ItemStack Item => new((IntPtr)T->DropGetItem(NativePtr));
}

public sealed unsafe class PlayerGameModeChangeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerGameModeChangeEvent(IntPtr ptr) : base(ptr) { }

    public GameMode NewGameMode => (GameMode)T->GmChangeGetNewMode(NativePtr);
}

public sealed unsafe class PlayerItemHeldEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerItemHeldEvent(IntPtr ptr) : base(ptr) { }

    public int PreviousSlot => T->HeldGetPreviousSlot(NativePtr);
    public int NewSlot => T->HeldGetNewSlot(NativePtr);
}

public sealed unsafe class PlayerItemConsumeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerItemConsumeEvent(IntPtr ptr) : base(ptr) { }

    public ItemStack Item => new((IntPtr)T->ConsumeGetItem(NativePtr));
    public EquipmentSlot Hand => (EquipmentSlot)T->ConsumeGetHand(NativePtr);
}

public sealed unsafe class PlayerKickEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerKickEvent(IntPtr ptr) : base(ptr) { }

    public string Reason
    {
        get => Bridge.Str(T->KickGetReason(NativePtr));
        set => Bridge.Call1(T->KickSetReason, NativePtr, value);
    }
}

public sealed unsafe class PlayerPickupItemEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerPickupItemEvent(IntPtr ptr) : base(ptr) { }

    public ItemStack Item => new((IntPtr)T->PickupGetItem(NativePtr), isItemActor: true);
}

public sealed unsafe class PlayerJumpEvent : Event
{
    internal PlayerJumpEvent(IntPtr ptr) : base(ptr) { }
}

public sealed unsafe class PlayerEmoteEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerEmoteEvent(IntPtr ptr) : base(ptr) { }

    public string EmoteId => Bridge.Str(T->EmoteGetId(NativePtr));
    public bool IsMuted => T->EmoteIsMuted(NativePtr);
    public void SetMuted(bool value) => T->EmoteSetMuted(NativePtr, value);
}

public sealed unsafe class PlayerBedEnterEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerBedEnterEvent(IntPtr ptr) : base(ptr) { }

    public Block Bed => new((IntPtr)Bridge.CallKindPtr(T->BedGetBed, NativePtr, EventKind.PlayerBedEnterEvent));
}

public sealed unsafe class PlayerBedLeaveEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerBedLeaveEvent(IntPtr ptr) : base(ptr) { }

    public Block Bed => new((IntPtr)Bridge.CallKindPtr(T->BedGetBed, NativePtr, EventKind.PlayerBedLeaveEvent));
}

public sealed unsafe class PlayerDimensionChangeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerDimensionChangeEvent(IntPtr ptr) : base(ptr) { }

    public string From => Bridge.Str(T->DimChangeGetFrom(NativePtr));
    public string To => Bridge.Str(T->DimChangeGetTo(NativePtr));
}

public sealed unsafe class PlayerSkinChangeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerSkinChangeEvent(IntPtr ptr) : base(ptr) { }

    /// <summary>The skin that will be applied. Reading it copies the native
    /// skin; cache the result if you need it more than once.</summary>
    public Skin NewSkin => new((IntPtr)T->SkinChangeGetNewSkin(NativePtr));

    public string? SkinChangeMessage
    {
        get
        {
            var p = T->SkinChangeGetMessage(NativePtr);
            return p == null ? null : Bridge.Str(p);
        }
        set => Bridge.Call1(T->SkinChangeSetMessage, NativePtr, value ?? string.Empty);
    }
}

// ==================== Actor events ====================

public sealed unsafe class ActorDamageEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorDamageEvent(IntPtr ptr) : base(ptr) { }

    public float Damage
    {
        get => T->ActorDamageGetDamage(NativePtr);
        set => T->ActorDamageSetDamage(NativePtr, value);
    }

    public DamageSource DamageSource => new((IntPtr)Bridge.CallKindPtr(T->EventGetDamageSource, NativePtr, EventKind.ActorDamageEvent));
}

public sealed unsafe class ActorDeathEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorDeathEvent(IntPtr ptr) : base(ptr) { }

    public DamageSource DamageSource => new((IntPtr)Bridge.CallKindPtr(T->EventGetDamageSource, NativePtr, EventKind.ActorDeathEvent));
}

public sealed unsafe class ActorExplodeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorExplodeEvent(IntPtr ptr) : base(ptr) { }

    public Location Location
    {
        get
        {
            var values = stackalloc float[5];
            T->ActorExplodeGetLocation(NativePtr, values);
            var dim = T->ActorExplodeGetDimension(NativePtr);
            var dimension = dim == null ? null : new Dimension((IntPtr)dim);
            return new Location(values[0], values[1], values[2], values[3], values[4], dimension);
        }
    }

    public int BlockCount => T->ActorExplodeGetBlockCount(NativePtr);

    public Block? GetBlock(int index)
    {
        var b = T->ActorExplodeGetBlock(NativePtr, index);
        return b == null ? null : new Block((IntPtr)b);
    }
}

public sealed unsafe class ActorKnockbackEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorKnockbackEvent(IntPtr ptr) : base(ptr) { }

    public Actor? Source
    {
        get
        {
            var a = T->ActorKnockbackGetSource(NativePtr);
            return a == null ? null : new Actor((IntPtr)a);
        }
    }

    public Vector3 Knockback
    {
        get => EventVectorHelper.Read(T->ActorKnockbackGetVector, NativePtr);
        set => EventVectorHelper.Write(T->ActorKnockbackSetVector, NativePtr, value);
    }
}

public sealed unsafe class ActorTeleportEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorTeleportEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->ActorTpGetFrom, T->ActorTpGetFromDimension, NativePtr);
        set => EventLocationHelper.Write(T->ActorTpSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->ActorTpGetTo, T->ActorTpGetToDimension, NativePtr);
        set => EventLocationHelper.Write(T->ActorTpSetTo, NativePtr, value);
    }
}

public sealed unsafe class ActorSpawnEvent : Event
{
    internal ActorSpawnEvent(IntPtr ptr) : base(ptr) { }
}

public sealed unsafe class ActorRemoveEvent : Event
{
    internal ActorRemoveEvent(IntPtr ptr) : base(ptr) { }
}

// ==================== Block events ====================

public sealed unsafe class BlockBreakEvent : Event
{
    internal BlockBreakEvent(IntPtr ptr) : base(ptr) { }
}

public sealed unsafe class BlockPlaceEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockPlaceEvent(IntPtr ptr) : base(ptr) { }

    public BlockState PlacedState => new((IntPtr)T->PlaceGetPlacedState(NativePtr));
    public Block BlockAgainst => new((IntPtr)T->PlaceGetAgainst(NativePtr));
}

public sealed unsafe class BlockCookEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockCookEvent(IntPtr ptr) : base(ptr) { }

    public ItemStack Source => new((IntPtr)T->CookGetSource(NativePtr));
    public ItemStack Result => new((IntPtr)T->CookGetResult(NativePtr));
}

public sealed unsafe class BlockExplodeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockExplodeEvent(IntPtr ptr) : base(ptr) { }

    public int BlockCount => T->BlockExplodeGetBlockCount(NativePtr);

    public Block? GetBlock(int index)
    {
        var b = T->BlockExplodeGetBlock(NativePtr, index);
        return b == null ? null : new Block((IntPtr)b);
    }
}

public sealed unsafe class BlockFormEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockFormEvent(IntPtr ptr) : base(ptr) { }

    public BlockState NewState => new((IntPtr)Bridge.CallKindPtr(T->GrowGetNewState, NativePtr, EventKind.BlockFormEvent));
}

public sealed unsafe class BlockGrowEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockGrowEvent(IntPtr ptr) : base(ptr) { }

    public BlockState NewState => new((IntPtr)Bridge.CallKindPtr(T->GrowGetNewState, NativePtr, EventKind.BlockGrowEvent));
}

public sealed unsafe class BlockFromToEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockFromToEvent(IntPtr ptr) : base(ptr) { }

    public Block ToBlock => new((IntPtr)T->FromToGetToBlock(NativePtr));
}

public sealed unsafe class BlockPistonExtendEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockPistonExtendEvent(IntPtr ptr) : base(ptr) { }

    public BlockFace Direction => (BlockFace)T->PistonGetDirection(NativePtr);
}

public sealed unsafe class BlockPistonRetractEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BlockPistonRetractEvent(IntPtr ptr) : base(ptr) { }

    public BlockFace Direction => (BlockFace)T->PistonGetDirection(NativePtr);
}

public sealed unsafe class LeavesDecayEvent : Event
{
    internal LeavesDecayEvent(IntPtr ptr) : base(ptr) { }
}

// ==================== Chunk events ====================

public sealed unsafe class ChunkLoadEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ChunkLoadEvent(IntPtr ptr) : base(ptr) { }

    public int X => Bridge.CallKindInt(T->ChunkGetX, NativePtr, EventKind.ChunkLoadEvent);
    public int Z => Bridge.CallKindInt(T->ChunkGetZ, NativePtr, EventKind.ChunkLoadEvent);
    public string DimensionName => Bridge.Str(Bridge.CallKindStr(T->ChunkGetDimensionName, NativePtr, EventKind.ChunkLoadEvent));
}

public sealed unsafe class ChunkUnloadEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ChunkUnloadEvent(IntPtr ptr) : base(ptr) { }

    public int X => Bridge.CallKindInt(T->ChunkGetX, NativePtr, EventKind.ChunkUnloadEvent);
    public int Z => Bridge.CallKindInt(T->ChunkGetZ, NativePtr, EventKind.ChunkUnloadEvent);
    public string DimensionName => Bridge.Str(Bridge.CallKindStr(T->ChunkGetDimensionName, NativePtr, EventKind.ChunkUnloadEvent));
}

// ==================== Server events ====================

public sealed unsafe class ServerCommandEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ServerCommandEvent(IntPtr ptr) : base(ptr) { }

    public string Command
    {
        get => Bridge.Str(T->ServerCmdGetCommand(NativePtr));
        set => Bridge.Call1(T->ServerCmdSetCommand, NativePtr, value);
    }

    public string SenderName => Bridge.Str(T->ServerCmdGetSenderName(NativePtr));

    public CommandSender Sender => new((IntPtr)T->ServerCmdGetSender(NativePtr));
}

public sealed unsafe class ServerLoadEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ServerLoadEvent(IntPtr ptr) : base(ptr) { }

    public LoadType Type => (LoadType)T->ServerLoadGetType(NativePtr);
}

public sealed unsafe class BroadcastMessageEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal BroadcastMessageEvent(IntPtr ptr) : base(ptr) { }

    public string Message
    {
        get => Bridge.Str(T->BroadcastGetMessage(NativePtr));
        set => Bridge.Call1(T->BroadcastSetMessage, NativePtr, value);
    }

    public int RecipientCount => T->BroadcastGetRecipientCount(NativePtr);
}

public sealed unsafe class ServerListPingEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ServerListPingEvent(IntPtr ptr) : base(ptr) { }

    public string Address => Bridge.Str(T->PingGetAddress(NativePtr));
    public string ServerGuid => Bridge.Str(T->PingGetServerGuid(NativePtr));
    public int LocalPort => T->PingGetLocalPort(NativePtr);
    public int LocalPortV6 => T->PingGetLocalPortV6(NativePtr);
    public string Motd => Bridge.Str(T->PingGetMotd(NativePtr));
    public int NetworkProtocolVersion => T->PingGetNetworkProtocolVersion(NativePtr);
    public string MinecraftVersionNetwork => Bridge.Str(T->PingGetMinecraftVersionNetwork(NativePtr));
    public int NumPlayers => T->PingGetNumPlayers(NativePtr);
    public int MaxPlayers => T->PingGetMaxPlayers(NativePtr);
    public string LevelName => Bridge.Str(T->PingGetLevelName(NativePtr));
    public GameMode GameMode => (GameMode)T->PingGetGameMode(NativePtr);

    public void SetServerGuid(string value) => Bridge.Call1(T->PingSetServerGuid, NativePtr, value);
    public void SetLocalPort(int value) => T->PingSetLocalPort(NativePtr, value);
    public void SetLocalPortV6(int value) => T->PingSetLocalPortV6(NativePtr, value);
    public void SetMotd(string value) => Bridge.Call1(T->PingSetMotd, NativePtr, value);
    public void SetMinecraftVersionNetwork(string value) => Bridge.Call1(T->PingSetMinecraftVersionNetwork, NativePtr, value);
    public void SetNumPlayers(int value) => T->PingSetNumPlayers(NativePtr, value);
    public void SetMaxPlayers(int value) => T->PingSetMaxPlayers(NativePtr, value);
    public void SetLevelName(string value) => Bridge.Call1(T->PingSetLevelName, NativePtr, value);
    public void SetGameMode(GameMode value) => T->PingSetGameMode(NativePtr, (int)value);
}

public sealed unsafe class PacketReceiveEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PacketReceiveEvent(IntPtr ptr) : base(ptr) { }

    public int PacketId => Bridge.CallKindInt(T->PacketGetId, NativePtr, EventKind.PacketReceiveEvent);

    public byte[] Payload
    {
        get
        {
            int len = 0;
            var data = Bridge.CallKindStr2(T->PacketGetPayload, NativePtr, EventKind.PacketReceiveEvent, &len);
            var result = new byte[len];
            if (len > 0)
            {
                Marshal.Copy((IntPtr)data, result, 0, len);
            }
            return result;
        }
        set
        {
            fixed (byte* p = value)
            {
                Bridge.CallKind3(T->PacketSetPayload, NativePtr, EventKind.PacketReceiveEvent, p, value.Length);
            }
        }
    }

    public new Player? Player
    {
        get
        {
            var p = Bridge.CallKindPtr(T->PacketGetPlayer, NativePtr, EventKind.PacketReceiveEvent);
            return p == null ? null : new Player((IntPtr)p);
        }
    }

    public string Address => Bridge.Str(Bridge.CallKindStr(T->PacketGetAddress, NativePtr, EventKind.PacketReceiveEvent));
    public int SubClientId => Bridge.CallKindInt(T->PacketGetSubClientId, NativePtr, EventKind.PacketReceiveEvent);
}

public sealed unsafe class PacketSendEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PacketSendEvent(IntPtr ptr) : base(ptr) { }

    public int PacketId => Bridge.CallKindInt(T->PacketGetId, NativePtr, EventKind.PacketSendEvent);

    public byte[] Payload
    {
        get
        {
            int len = 0;
            var data = Bridge.CallKindStr2(T->PacketGetPayload, NativePtr, EventKind.PacketSendEvent, &len);
            var result = new byte[len];
            if (len > 0)
            {
                Marshal.Copy((IntPtr)data, result, 0, len);
            }
            return result;
        }
        set
        {
            fixed (byte* p = value)
            {
                Bridge.CallKind3(T->PacketSetPayload, NativePtr, EventKind.PacketSendEvent, p, value.Length);
            }
        }
    }

    public new Player? Player
    {
        get
        {
            var p = Bridge.CallKindPtr(T->PacketGetPlayer, NativePtr, EventKind.PacketSendEvent);
            return p == null ? null : new Player((IntPtr)p);
        }
    }

    public string Address => Bridge.Str(Bridge.CallKindStr(T->PacketGetAddress, NativePtr, EventKind.PacketSendEvent));
    public int SubClientId => Bridge.CallKindInt(T->PacketGetSubClientId, NativePtr, EventKind.PacketSendEvent);
}

public sealed unsafe class PluginEnableEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PluginEnableEvent(IntPtr ptr) : base(ptr) { }

    /// <summary>Gets the plugin involved in this event.</summary>
    public Plugin Plugin => Plugin.FromNative((IntPtr)Bridge.CallKindPtr(T->PluginEventGetPlugin, NativePtr, EventKind.PluginEnableEvent));

    public string PluginName => Plugin.Name;
}

public sealed unsafe class PluginDisableEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PluginDisableEvent(IntPtr ptr) : base(ptr) { }

    /// <summary>Gets the plugin involved in this event.</summary>
    public Plugin Plugin => Plugin.FromNative((IntPtr)Bridge.CallKindPtr(T->PluginEventGetPlugin, NativePtr, EventKind.PluginDisableEvent));

    public string PluginName => Plugin.Name;
}

public sealed unsafe class ScriptMessageEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ScriptMessageEvent(IntPtr ptr) : base(ptr) { }

    public string MessageId => Bridge.Str(T->ScriptGetMessageId(NativePtr));
    public string Message => Bridge.Str(T->ScriptGetMessage(NativePtr));
    public string SenderName => Bridge.Str(T->ScriptGetSenderName(NativePtr));
}

public sealed unsafe class MapInitializeEvent : Event
{
    internal MapInitializeEvent(IntPtr ptr) : base(ptr) { }
}

// ==================== Weather events ====================

public sealed unsafe class ThunderChangeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ThunderChangeEvent(IntPtr ptr) : base(ptr) { }

    public bool ToThunderState => T->ThunderChangeGetTo(NativePtr);
}

public sealed unsafe class WeatherChangeEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal WeatherChangeEvent(IntPtr ptr) : base(ptr) { }

    public bool ToWeatherState => T->WeatherChangeGetTo(NativePtr);
}

/// <summary>Creates typed event wrappers from the native event pointer.</summary>
internal static class EventFactory
{
    /// <summary>
    /// The built-in events this loader can wrap, keyed by event name. This is the
    /// single source of truth for both dispatch and registration: a name outside
    /// this table belongs to no built-in event, so it is rejected at registration
    /// rather than reaching a handler with no readable payload or cancellation state.
    /// </summary>
    private static readonly FrozenDictionary<string, Func<IntPtr, Event>> Factories =
        new Dictionary<string, Func<IntPtr, Event>>(StringComparer.Ordinal)
        {
            [nameof(PlayerJoinEvent)] = p => new PlayerJoinEvent(p) { Kind = EventKind.PlayerJoinEvent },
            [nameof(PlayerQuitEvent)] = p => new PlayerQuitEvent(p) { Kind = EventKind.PlayerQuitEvent },
            [nameof(PlayerLoginEvent)] = p => new PlayerLoginEvent(p) { Kind = EventKind.PlayerLoginEvent },
            [nameof(PlayerChatEvent)] = p => new PlayerChatEvent(p) { Kind = EventKind.PlayerChatEvent },
            [nameof(PlayerCommandEvent)] = p => new PlayerCommandEvent(p) { Kind = EventKind.PlayerCommandEvent },
            [nameof(PlayerMoveEvent)] = p => new PlayerMoveEvent(p) { Kind = EventKind.PlayerMoveEvent },
            [nameof(PlayerTeleportEvent)] = p => new PlayerTeleportEvent(p) { Kind = EventKind.PlayerTeleportEvent },
            [nameof(PlayerPortalEvent)] = p => new PlayerPortalEvent(p) { Kind = EventKind.PlayerPortalEvent },
            [nameof(PlayerDeathEvent)] = p => new PlayerDeathEvent(p) { Kind = EventKind.PlayerDeathEvent },
            [nameof(PlayerInteractEvent)] = p => new PlayerInteractEvent(p) { Kind = EventKind.PlayerInteractEvent },
            [nameof(PlayerInteractActorEvent)] = p => new PlayerInteractActorEvent(p) { Kind = EventKind.PlayerInteractActorEvent },
            [nameof(PlayerRespawnEvent)] = p => new PlayerRespawnEvent(p) { Kind = EventKind.PlayerRespawnEvent },
            [nameof(PlayerDropItemEvent)] = p => new PlayerDropItemEvent(p) { Kind = EventKind.PlayerDropItemEvent },
            [nameof(PlayerGameModeChangeEvent)] = p => new PlayerGameModeChangeEvent(p) { Kind = EventKind.PlayerGameModeChangeEvent },
            [nameof(PlayerItemHeldEvent)] = p => new PlayerItemHeldEvent(p) { Kind = EventKind.PlayerItemHeldEvent },
            [nameof(PlayerItemConsumeEvent)] = p => new PlayerItemConsumeEvent(p) { Kind = EventKind.PlayerItemConsumeEvent },
            [nameof(PlayerKickEvent)] = p => new PlayerKickEvent(p) { Kind = EventKind.PlayerKickEvent },
            [nameof(PlayerPickupItemEvent)] = p => new PlayerPickupItemEvent(p) { Kind = EventKind.PlayerPickupItemEvent },
            [nameof(PlayerJumpEvent)] = p => new PlayerJumpEvent(p) { Kind = EventKind.PlayerJumpEvent },
            [nameof(PlayerEmoteEvent)] = p => new PlayerEmoteEvent(p) { Kind = EventKind.PlayerEmoteEvent },
            [nameof(PlayerBedEnterEvent)] = p => new PlayerBedEnterEvent(p) { Kind = EventKind.PlayerBedEnterEvent },
            [nameof(PlayerBedLeaveEvent)] = p => new PlayerBedLeaveEvent(p) { Kind = EventKind.PlayerBedLeaveEvent },
            [nameof(PlayerDimensionChangeEvent)] = p => new PlayerDimensionChangeEvent(p) { Kind = EventKind.PlayerDimensionChangeEvent },
            [nameof(PlayerSkinChangeEvent)] = p => new PlayerSkinChangeEvent(p) { Kind = EventKind.PlayerSkinChangeEvent },
            [nameof(ActorDamageEvent)] = p => new ActorDamageEvent(p) { Kind = EventKind.ActorDamageEvent },
            [nameof(ActorDeathEvent)] = p => new ActorDeathEvent(p) { Kind = EventKind.ActorDeathEvent },
            [nameof(ActorExplodeEvent)] = p => new ActorExplodeEvent(p) { Kind = EventKind.ActorExplodeEvent },
            [nameof(ActorKnockbackEvent)] = p => new ActorKnockbackEvent(p) { Kind = EventKind.ActorKnockbackEvent },
            [nameof(ActorTeleportEvent)] = p => new ActorTeleportEvent(p) { Kind = EventKind.ActorTeleportEvent },
            [nameof(ActorSpawnEvent)] = p => new ActorSpawnEvent(p) { Kind = EventKind.ActorSpawnEvent },
            [nameof(ActorRemoveEvent)] = p => new ActorRemoveEvent(p) { Kind = EventKind.ActorRemoveEvent },
            [nameof(BlockBreakEvent)] = p => new BlockBreakEvent(p) { Kind = EventKind.BlockBreakEvent },
            [nameof(BlockPlaceEvent)] = p => new BlockPlaceEvent(p) { Kind = EventKind.BlockPlaceEvent },
            [nameof(BlockCookEvent)] = p => new BlockCookEvent(p) { Kind = EventKind.BlockCookEvent },
            [nameof(BlockExplodeEvent)] = p => new BlockExplodeEvent(p) { Kind = EventKind.BlockExplodeEvent },
            [nameof(BlockFormEvent)] = p => new BlockFormEvent(p) { Kind = EventKind.BlockFormEvent },
            [nameof(BlockGrowEvent)] = p => new BlockGrowEvent(p) { Kind = EventKind.BlockGrowEvent },
            [nameof(BlockFromToEvent)] = p => new BlockFromToEvent(p) { Kind = EventKind.BlockFromToEvent },
            [nameof(BlockPistonExtendEvent)] = p => new BlockPistonExtendEvent(p) { Kind = EventKind.BlockPistonExtendEvent },
            [nameof(BlockPistonRetractEvent)] = p => new BlockPistonRetractEvent(p) { Kind = EventKind.BlockPistonRetractEvent },
            [nameof(LeavesDecayEvent)] = p => new LeavesDecayEvent(p) { Kind = EventKind.LeavesDecayEvent },
            [nameof(ChunkLoadEvent)] = p => new ChunkLoadEvent(p) { Kind = EventKind.ChunkLoadEvent },
            [nameof(ChunkUnloadEvent)] = p => new ChunkUnloadEvent(p) { Kind = EventKind.ChunkUnloadEvent },
            [nameof(ServerCommandEvent)] = p => new ServerCommandEvent(p) { Kind = EventKind.ServerCommandEvent },
            [nameof(ServerLoadEvent)] = p => new ServerLoadEvent(p) { Kind = EventKind.ServerLoadEvent },
            [nameof(BroadcastMessageEvent)] = p => new BroadcastMessageEvent(p) { Kind = EventKind.BroadcastMessageEvent },
            [nameof(ServerListPingEvent)] = p => new ServerListPingEvent(p) { Kind = EventKind.ServerListPingEvent },
            [nameof(PacketReceiveEvent)] = p => new PacketReceiveEvent(p) { Kind = EventKind.PacketReceiveEvent },
            [nameof(PacketSendEvent)] = p => new PacketSendEvent(p) { Kind = EventKind.PacketSendEvent },
            [nameof(PluginEnableEvent)] = p => new PluginEnableEvent(p) { Kind = EventKind.PluginEnableEvent },
            [nameof(PluginDisableEvent)] = p => new PluginDisableEvent(p) { Kind = EventKind.PluginDisableEvent },
            [nameof(ScriptMessageEvent)] = p => new ScriptMessageEvent(p) { Kind = EventKind.ScriptMessageEvent },
            [nameof(MapInitializeEvent)] = p => new MapInitializeEvent(p) { Kind = EventKind.MapInitializeEvent },
            [nameof(ThunderChangeEvent)] = p => new ThunderChangeEvent(p) { Kind = EventKind.ThunderChangeEvent },
            [nameof(WeatherChangeEvent)] = p => new WeatherChangeEvent(p) { Kind = EventKind.WeatherChangeEvent },
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>True when <paramref name="eventName"/> maps to one of the
    /// built-in event wrappers.</summary>
    internal static bool IsKnown(string eventName) => Factories.ContainsKey(eventName);

    /// <summary>Wraps a native event. The caller must have checked
    /// <see cref="IsKnown"/>.</summary>
    internal static Event Create(string eventName, IntPtr eventPtr) => Factories[eventName](eventPtr);
}

/// <summary>
/// Per-plugin event manager. Registering an event immediately forwards it to
/// the native side (the C++ loader flushes registrations into the Endstone
/// plugin manager right after the managed OnEnable completes).
/// </summary>
internal sealed unsafe class EventManager
{
    private readonly List<GCHandle> _callbackHandles = [];
    private IntPtr _pluginHandle;

    internal void SetPluginHandle(IntPtr gcHandle) => _pluginHandle = gcHandle;

    /// <summary>Registers <paramref name="handler"/> for the built-in event
    /// class <paramref name="eventName"/>. Returns false when the name is not
    /// one of the built-in events, in which case nothing is registered.</summary>
    internal bool Register(string eventName, EventPriority priority, bool ignoreCancelled, Action<Event> handler)
    {
        if (!EventFactory.IsKnown(eventName))
        {
            return false;
        }
        // The native callback receives the raw Event*; wrap it per event name.
        Action<IntPtr> native = ptr => handler(EventFactory.Create(eventName, ptr));
        var handle = GCHandle.Alloc(native);
        _callbackHandles.Add(handle);  // keep the delegate alive for the plugin's lifetime
        if (_pluginHandle != IntPtr.Zero)
        {
            Bridge.CallRegisterEvent((void*)_pluginHandle, eventName, (int)priority, ignoreCancelled,
                                     (void*)GCHandle.ToIntPtr(handle));
        }
        return true;
    }
}
