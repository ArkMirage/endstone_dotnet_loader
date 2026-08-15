namespace Endstone.Loader;

public enum BarColor
{
    Pink = 0,
    Blue = 1,
    Red = 2,
    Green = 3,
    Yellow = 4,
    Purple = 5,
    RebeccaPurple = 6,
    White = 7,
}

public enum BarStyle
{
    Solid = 0,
    Segmented6 = 1,
    Segmented10 = 2,
    Segmented12 = 3,
    Segmented20 = 4,
}

[Flags]
public enum BarFlag
{
    None = 0,
    DarkenSky = 1,
    CreateFog = 2,
}

/// <summary>
/// A boss bar displayed to players at the top of the screen. Created via
/// Server.CreateBossBar; Dispose() frees the native object (bar disappears).
/// </summary>
public sealed unsafe class BossBar : IDisposable
{
    private static Bridge.Table* T => Bridge.Raw;

    private void* _ptr;

    internal BossBar(void* ptr) => _ptr = ptr;

    internal void* NativePtr => _ptr;

    public string Title
    {
        get => Bridge.Str(T->BossBarGetTitle(_ptr));
        set => Bridge.Call1(T->BossBarSetTitle, _ptr, value);
    }

    public BarColor Color
    {
        get => (BarColor)T->BossBarGetColor(_ptr);
        set => T->BossBarSetColor(_ptr, (int)value);
    }

    public BarStyle Style
    {
        get => (BarStyle)T->BossBarGetStyle(_ptr);
        set => T->BossBarSetStyle(_ptr, (int)value);
    }

    public float Progress
    {
        get => T->BossBarGetProgress(_ptr);
        set => T->BossBarSetProgress(_ptr, value);
    }

    public bool IsVisible
    {
        get => T->BossBarIsVisible(_ptr);
        set => T->BossBarSetVisible(_ptr, value);
    }

    public bool HasFlag(BarFlag flag) => T->BossBarHasFlag(_ptr, (int)flag);

    public void AddFlag(BarFlag flag) => T->BossBarAddFlag(_ptr, (int)flag);

    public void RemoveFlag(BarFlag flag) => T->BossBarRemoveFlag(_ptr, (int)flag);

    public void AddPlayer(Player player) => T->BossBarAddPlayer(_ptr, (void*)player.NativePtr);

    public void RemovePlayer(Player player) => T->BossBarRemovePlayer(_ptr, (void*)player.NativePtr);

    public void RemoveAll() => T->BossBarRemoveAll(_ptr);

    public Player[] Players
    {
        get
        {
            var count = T->BossBarGetPlayerCount(_ptr);
            var players = new Player[count];
            for (var i = 0; i < count; i++)
            {
                players[i] = new Player((IntPtr)T->BossBarGetPlayer(_ptr, i));
            }
            return players;
        }
    }

    public void Dispose()
    {
        if (_ptr == null)
        {
            return;
        }
        T->BossBarDestroy(_ptr);
        _ptr = null;
    }
}
