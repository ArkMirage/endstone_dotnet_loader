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

public readonly struct Location
{
    public Location(float x, float y, float z, float pitch = 0, float yaw = 0)
    {
        X = x;
        Y = y;
        Z = z;
        Pitch = pitch;
        Yaw = yaw;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float Pitch { get; }
    public float Yaw { get; }

    public override string ToString() => $"({X}, {Y}, {Z})";

    public int GetBlockX() => (int)MathF.Floor(X);
    public int GetBlockY() => (int)MathF.Floor(Y);
    public int GetBlockZ() => (int)MathF.Floor(Z);
}

internal static class EventLocationHelper
{
    internal static unsafe Location Read(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr)
    {
        var values = stackalloc float[5];
        fn(ptr, values);
        return new Location(values[0], values[1], values[2], values[3], values[4]);
    }

    internal static unsafe void Write(delegate* unmanaged[Cdecl]<void*, float*, void> fn, void* ptr, Location loc)
    {
        var values = stackalloc float[5] { loc.X, loc.Y, loc.Z, loc.Pitch, loc.Yaw };
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
        get => EventLocationHelper.Read(T->MoveGetFrom, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetTo, NativePtr, value);
    }
}

public sealed unsafe class PlayerTeleportEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerTeleportEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->MoveGetFrom, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetTo, NativePtr, value);
    }
}

public sealed unsafe class PlayerPortalEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal PlayerPortalEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->MoveGetFrom, NativePtr);
        set => EventLocationHelper.Write(T->MoveSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->MoveGetTo, NativePtr);
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

    public Location? ClickedPosition
    {
        get
        {
            var values = stackalloc float[3];
            if (T->InteractGetClickedPosition(NativePtr, values) == 0)
            {
                return null;
            }
            return new Location(values[0], values[1], values[2]);
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

    public string NewSkinId => Bridge.Str(T->SkinChangeGetNewSkinId(NativePtr));

    public string? NewSkinCapeId
    {
        get
        {
            var p = T->SkinChangeGetNewSkinCapeId(NativePtr);
            return p == null ? null : Bridge.Str(p);
        }
    }

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
            return new Location(values[0], values[1], values[2], values[3], values[4]);
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

    public Location Knockback
    {
        get
        {
            var values = stackalloc float[3];
            T->ActorKnockbackGetVector(NativePtr, values);
            return new Location(values[0], values[1], values[2]);
        }
        set
        {
            var values = stackalloc float[3] { value.X, value.Y, value.Z };
            T->ActorKnockbackSetVector(NativePtr, values);
        }
    }
}

public sealed unsafe class ActorTeleportEvent : Event
{
    private static Bridge.Table* T => Bridge.Raw;
    internal ActorTeleportEvent(IntPtr ptr) : base(ptr) { }

    public Location From
    {
        get => EventLocationHelper.Read(T->ActorTpGetFrom, NativePtr);
        set => EventLocationHelper.Write(T->ActorTpSetFrom, NativePtr, value);
    }

    public Location To
    {
        get => EventLocationHelper.Read(T->ActorTpGetTo, NativePtr);
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
    internal static Event? Create(string eventName, IntPtr eventPtr)
    {
        return eventName switch
        {
            "PlayerJoinEvent" => new PlayerJoinEvent(eventPtr) { Kind = EventKind.PlayerJoinEvent },
            "PlayerQuitEvent" => new PlayerQuitEvent(eventPtr) { Kind = EventKind.PlayerQuitEvent },
            "PlayerLoginEvent" => new PlayerLoginEvent(eventPtr) { Kind = EventKind.PlayerLoginEvent },
            "PlayerChatEvent" => new PlayerChatEvent(eventPtr) { Kind = EventKind.PlayerChatEvent },
            "PlayerCommandEvent" => new PlayerCommandEvent(eventPtr) { Kind = EventKind.PlayerCommandEvent },
            "PlayerMoveEvent" => new PlayerMoveEvent(eventPtr) { Kind = EventKind.PlayerMoveEvent },
            "PlayerTeleportEvent" => new PlayerTeleportEvent(eventPtr) { Kind = EventKind.PlayerTeleportEvent },
            "PlayerPortalEvent" => new PlayerPortalEvent(eventPtr) { Kind = EventKind.PlayerPortalEvent },
            "PlayerDeathEvent" => new PlayerDeathEvent(eventPtr) { Kind = EventKind.PlayerDeathEvent },
            "PlayerInteractEvent" => new PlayerInteractEvent(eventPtr) { Kind = EventKind.PlayerInteractEvent },
            "PlayerInteractActorEvent" => new PlayerInteractActorEvent(eventPtr) { Kind = EventKind.PlayerInteractActorEvent },
            "PlayerRespawnEvent" => new PlayerRespawnEvent(eventPtr) { Kind = EventKind.PlayerRespawnEvent },
            "PlayerDropItemEvent" => new PlayerDropItemEvent(eventPtr) { Kind = EventKind.PlayerDropItemEvent },
            "PlayerGameModeChangeEvent" => new PlayerGameModeChangeEvent(eventPtr) { Kind = EventKind.PlayerGameModeChangeEvent },
            "PlayerItemHeldEvent" => new PlayerItemHeldEvent(eventPtr) { Kind = EventKind.PlayerItemHeldEvent },
            "PlayerItemConsumeEvent" => new PlayerItemConsumeEvent(eventPtr) { Kind = EventKind.PlayerItemConsumeEvent },
            "PlayerKickEvent" => new PlayerKickEvent(eventPtr) { Kind = EventKind.PlayerKickEvent },
            "PlayerPickupItemEvent" => new PlayerPickupItemEvent(eventPtr) { Kind = EventKind.PlayerPickupItemEvent },
            "PlayerJumpEvent" => new PlayerJumpEvent(eventPtr) { Kind = EventKind.PlayerJumpEvent },
            "PlayerEmoteEvent" => new PlayerEmoteEvent(eventPtr) { Kind = EventKind.PlayerEmoteEvent },
            "PlayerBedEnterEvent" => new PlayerBedEnterEvent(eventPtr) { Kind = EventKind.PlayerBedEnterEvent },
            "PlayerBedLeaveEvent" => new PlayerBedLeaveEvent(eventPtr) { Kind = EventKind.PlayerBedLeaveEvent },
            "PlayerDimensionChangeEvent" => new PlayerDimensionChangeEvent(eventPtr) { Kind = EventKind.PlayerDimensionChangeEvent },
            "PlayerSkinChangeEvent" => new PlayerSkinChangeEvent(eventPtr) { Kind = EventKind.PlayerSkinChangeEvent },
            "ActorDamageEvent" => new ActorDamageEvent(eventPtr) { Kind = EventKind.ActorDamageEvent },
            "ActorDeathEvent" => new ActorDeathEvent(eventPtr) { Kind = EventKind.ActorDeathEvent },
            "ActorExplodeEvent" => new ActorExplodeEvent(eventPtr) { Kind = EventKind.ActorExplodeEvent },
            "ActorKnockbackEvent" => new ActorKnockbackEvent(eventPtr) { Kind = EventKind.ActorKnockbackEvent },
            "ActorTeleportEvent" => new ActorTeleportEvent(eventPtr) { Kind = EventKind.ActorTeleportEvent },
            "ActorSpawnEvent" => new ActorSpawnEvent(eventPtr) { Kind = EventKind.ActorSpawnEvent },
            "ActorRemoveEvent" => new ActorRemoveEvent(eventPtr) { Kind = EventKind.ActorRemoveEvent },
            "BlockBreakEvent" => new BlockBreakEvent(eventPtr) { Kind = EventKind.BlockBreakEvent },
            "BlockPlaceEvent" => new BlockPlaceEvent(eventPtr) { Kind = EventKind.BlockPlaceEvent },
            "BlockCookEvent" => new BlockCookEvent(eventPtr) { Kind = EventKind.BlockCookEvent },
            "BlockExplodeEvent" => new BlockExplodeEvent(eventPtr) { Kind = EventKind.BlockExplodeEvent },
            "BlockFormEvent" => new BlockFormEvent(eventPtr) { Kind = EventKind.BlockFormEvent },
            "BlockGrowEvent" => new BlockGrowEvent(eventPtr) { Kind = EventKind.BlockGrowEvent },
            "BlockFromToEvent" => new BlockFromToEvent(eventPtr) { Kind = EventKind.BlockFromToEvent },
            "BlockPistonExtendEvent" => new BlockPistonExtendEvent(eventPtr) { Kind = EventKind.BlockPistonExtendEvent },
            "BlockPistonRetractEvent" => new BlockPistonRetractEvent(eventPtr) { Kind = EventKind.BlockPistonRetractEvent },
            "LeavesDecayEvent" => new LeavesDecayEvent(eventPtr) { Kind = EventKind.LeavesDecayEvent },
            "ChunkLoadEvent" => new ChunkLoadEvent(eventPtr) { Kind = EventKind.ChunkLoadEvent },
            "ChunkUnloadEvent" => new ChunkUnloadEvent(eventPtr) { Kind = EventKind.ChunkUnloadEvent },
            "ServerCommandEvent" => new ServerCommandEvent(eventPtr) { Kind = EventKind.ServerCommandEvent },
            "ServerLoadEvent" => new ServerLoadEvent(eventPtr) { Kind = EventKind.ServerLoadEvent },
            "BroadcastMessageEvent" => new BroadcastMessageEvent(eventPtr) { Kind = EventKind.BroadcastMessageEvent },
            "ServerListPingEvent" => new ServerListPingEvent(eventPtr) { Kind = EventKind.ServerListPingEvent },
            "PacketReceiveEvent" => new PacketReceiveEvent(eventPtr) { Kind = EventKind.PacketReceiveEvent },
            "PacketSendEvent" => new PacketSendEvent(eventPtr) { Kind = EventKind.PacketSendEvent },
            "PluginEnableEvent" => new PluginEnableEvent(eventPtr) { Kind = EventKind.PluginEnableEvent },
            "PluginDisableEvent" => new PluginDisableEvent(eventPtr) { Kind = EventKind.PluginDisableEvent },
            "ScriptMessageEvent" => new ScriptMessageEvent(eventPtr) { Kind = EventKind.ScriptMessageEvent },
            "MapInitializeEvent" => new MapInitializeEvent(eventPtr) { Kind = EventKind.MapInitializeEvent },
            "ThunderChangeEvent" => new ThunderChangeEvent(eventPtr) { Kind = EventKind.ThunderChangeEvent },
            "WeatherChangeEvent" => new WeatherChangeEvent(eventPtr) { Kind = EventKind.WeatherChangeEvent },
            _ => null,
        };
    }
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

    internal void Register(string eventName, EventPriority priority, bool ignoreCancelled, Action<Event> handler)
    {
        // The native callback receives the raw Event*; wrap it per event name.
        Action<IntPtr> native = ptr =>
        {
            var e = EventFactory.Create(eventName, ptr);
            if (e != null)
            {
                handler(e);
            }
        };
        var handle = GCHandle.Alloc(native);
        _callbackHandles.Add(handle);  // keep the delegate alive for the plugin's lifetime
        if (_pluginHandle != IntPtr.Zero)
        {
            Bridge.CallRegisterEvent((void*)_pluginHandle, eventName, (int)priority, ignoreCancelled,
                                     (void*)GCHandle.ToIntPtr(handle));
        }
    }
}