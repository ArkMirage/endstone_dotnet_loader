using System.Runtime.InteropServices;

namespace Endstone.Loader;

public enum GameMode
{
    Survival = 0,
    Creative = 1,
    Adventure = 2,
    Spectator = 3,
}

/// <summary>Wrapper around a native endstone::Player (also an Actor).</summary>
public sealed unsafe class Player : Actor
{
    internal Player(IntPtr ptr) : base(ptr) { }

    private static Bridge.Table* T => Bridge.Raw;

    public new string Name => Bridge.Str(T->PlayerGetName(_ptr));
    public string Xuid => Bridge.Str(T->PlayerGetXuid(_ptr));
    public string Address => Bridge.Str(T->PlayerGetAddress(_ptr));
    public string Locale => Bridge.Str(T->PlayerGetLocale(_ptr));
    public string DeviceOS => Bridge.Str(T->PlayerGetDeviceOS(_ptr));
    public string DeviceId => Bridge.Str(T->PlayerGetDeviceId(_ptr));
    public string GameVersion => Bridge.Str(T->PlayerGetGameVersion(_ptr));
    public string SkinId => Bridge.Str(T->PlayerGetSkinId(_ptr));
    public string? SkinCapeId
    {
        get
        {
            var p = T->PlayerGetSkinCapeId(_ptr);
            return p == null ? null : Bridge.Str(p);
        }
    }

    /// <summary>Item held in the main hand, or null when empty. Read-only snapshot.</summary>
    public ItemStack? ItemInHand
    {
        get
        {
            var p = T->PlayerGetItemInHand(_ptr);
            return p == null ? null : new ItemStack((IntPtr)p);
        }
    }
    public int Ping => T->PlayerGetPing(_ptr);
    public bool IsOp => T->PlayerIsOp(_ptr);
    public bool IsSneaking => T->PlayerIsSneaking(_ptr);
    public bool IsSprinting => T->PlayerIsSprinting(_ptr);
    public bool AllowFlight => T->PlayerGetAllowFlight(_ptr);
    public bool IsFlying => T->PlayerIsFlying(_ptr);
    public int ExpLevel => T->PlayerGetExpLevel(_ptr);
    public int TotalExp => T->PlayerGetTotalExp(_ptr);
    public float ExpProgress => T->PlayerGetExpProgress(_ptr);
    public float FlySpeed => T->PlayerGetFlySpeed(_ptr);
    public float WalkSpeed => T->PlayerGetWalkSpeed(_ptr);
    public GameMode GameMode => (GameMode)T->PlayerGetGameMode(_ptr);

    /// <summary>The player's inventory (hotbar + armor + hands).</summary>
    public PlayerInventory Inventory => new((IntPtr)T->PlayerGetInventory(_ptr));
    /// <summary>The player's ender chest inventory.</summary>
    public Inventory EnderChest => new((IntPtr)T->PlayerGetEnderChest(_ptr));
    public new void SendMessage(string message) => Bridge.Call1(T->PlayerSendMessage, _ptr, message);

    public new void SendMessage(string format, params object?[] args) => SendMessage(string.Format(format, args));

    public void SendErrorMessage(string message) => Bridge.Call1(T->PlayerSendErrorMessage, _ptr, message);

    public void SendErrorMessage(string format, params object?[] args) => SendErrorMessage(string.Format(format, args));
    public void SendPopup(string message) => Bridge.Call1(T->PlayerSendPopup, _ptr, message);
    public void SendTip(string message) => Bridge.Call1(T->PlayerSendTip, _ptr, message);
    public void SendToast(string title, string content) => Bridge.Call2(T->PlayerSendToast, _ptr, title, content);
    public void SendTitle(string title, string subtitle) => Bridge.Call2(T->PlayerSendTitle, _ptr, title, subtitle);
    public void ResetTitle() => T->PlayerResetTitle(_ptr);
    public void Kick(string reason) => Bridge.Call1(T->PlayerKick, _ptr, reason);
    public bool PerformCommand(string command) => Bridge.CallBoolStr(T->PlayerPerformCommand, _ptr, command);
    public void Transfer(string host, int port) => Bridge.CallHostPort(T->PlayerTransfer, _ptr, host, port);

    public void PlaySound(Location location, string sound, float volume = 1.0f, float pitch = 1.0f)
        => Bridge.CallSound(T->PlayerPlaySound, _ptr, location, sound, volume, pitch);
    public void StopSound(string sound) => Bridge.Call1(T->PlayerStopSound, _ptr, sound);
    public void StopAllSounds() => T->PlayerStopAllSounds(_ptr);

    public void SpawnParticle(string name, Location location)
        => Bridge.CallParticle(T->PlayerSpawnParticle, _ptr, name, location, null);
    public void SpawnParticle(string name, Location location, string molangVariablesJson)
        => Bridge.CallParticle(T->PlayerSpawnParticle, _ptr, name, location, molangVariablesJson);

    public void SetOp(bool value) => T->PlayerSetOp(_ptr, value);
    public void SetSneaking(bool value) => T->PlayerSetSneaking(_ptr, value);
    public void SetSprinting(bool value) => T->PlayerSetSprinting(_ptr, value);
    public void SetAllowFlight(bool value) => T->PlayerSetAllowFlight(_ptr, value);
    public void SetFlying(bool value) => T->PlayerSetFlying(_ptr, value);
    public void SetFlySpeed(float value) => T->PlayerSetFlySpeed(_ptr, value);
    public void SetWalkSpeed(float value) => T->PlayerSetWalkSpeed(_ptr, value);
    public void SetGameMode(GameMode mode) => T->PlayerSetGameMode(_ptr, (int)mode);
    public void SetExpLevel(int level) => T->PlayerSetExpLevel(_ptr, level);
    public void GiveExp(int amount) => T->PlayerGiveExp(_ptr, amount);
    public void GiveExpLevels(int amount) => T->PlayerGiveExpLevels(_ptr, amount);
    public void SetExpProgress(float progress) => T->PlayerSetExpProgress(_ptr, progress);
    public void UpdateCommands() => T->PlayerUpdateCommands(_ptr);
    public void CloseForm() => T->PlayerCloseForm(_ptr);

    public void ShowForm<T>(FormBase<T> form) where T : FormBase<T> => form.Send(this);

    public void SendPacket(int packetId, ReadOnlySpan<byte> payload)
    {
        fixed (byte* p = payload)
        {
            T->PlayerSendPacket(_ptr, packetId, p, payload.Length);
        }
    }

    /// <summary>Sends the full map rendering (pixels + cursors) to this player.
    /// Blocks the server thread while the renderers draw.</summary>
    public void SendMap(MapView map) => T->PlayerSendMap(_ptr, (void*)map.NativePtr);
}