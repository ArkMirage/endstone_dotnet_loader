namespace Endstone.Loader;

/// <summary>Wrapper around the native endstone::Server singleton.</summary>
public sealed unsafe class Server
{
    private readonly void* _ptr;

    internal Server(IntPtr ptr) => _ptr = (void*)ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public string Name => Bridge.Str(T->ServerGetName(_ptr));
    public string Version => Bridge.Str(T->ServerGetVersion(_ptr));
    public string MinecraftVersion => Bridge.Str(T->ServerGetMinecraftVersion(_ptr));
    public int ProtocolVersion => T->ServerGetProtocolVersion(_ptr);
    public int MaxPlayers => T->ServerGetMaxPlayers(_ptr);

    public void BroadcastMessage(string message) => Bridge.Call1(T->ServerBroadcastMessage, _ptr, message);

    public Player[] GetOnlinePlayers()
    {
        const int capacity = 256;
        var buffer = stackalloc void*[capacity];
        var count = T->ServerGetOnlinePlayers(_ptr, buffer, capacity);
        var players = new Player[count];
        for (var i = 0; i < count; i++)
        {
            players[i] = new Player((IntPtr)buffer[i]);
        }
        return players;
    }

    public Player? GetPlayer(string name)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(name + "\0");
        fixed (byte* p = buf)
        {
            var player = T->ServerGetPlayer(_ptr, p);
            return player == null ? null : new Player((IntPtr)player);
        }
    }

    /// <summary>Gets the console command sender.</summary>
    public ConsoleCommandSender ConsoleSender => new((IntPtr)T->ServerGetConsoleSender(_ptr));

    public bool DispatchCommand(string commandLine)
    {
        var console = T->ServerGetConsoleSender(_ptr);
        var buf = System.Text.Encoding.UTF8.GetBytes(commandLine + "\0");
        fixed (byte* p = buf)
        {
            return T->ServerDispatchCommand(_ptr, console, p);
        }
    }

    /// <summary>Gets the level of this server, or null before the level has loaded.</summary>
    public Level? Level
    {
        get
        {
            var level = T->ServerGetLevel(_ptr);
            return level == null ? null : new Level((IntPtr)level);
        }
    }

    /// <summary>Gets the map view with the given item ID, or null if it does not exist.</summary>
    public MapView? GetMap(long id)
    {
        var map = T->ServerGetMap(_ptr, id);
        return map == null ? null : new MapView((IntPtr)map);
    }

    /// <summary>Creates a new map view (automatically assigned ID) for the given dimension.</summary>
    public MapView? CreateMap(Dimension dimension)
    {
        var map = T->ServerCreateMap(_ptr, (void*)dimension.NativePtr);
        return map == null ? null : new MapView((IntPtr)map);
    }

    /// <summary>Creates a boss bar (progress defaults to 1.0, visible by default).</summary>
    public BossBar? CreateBossBar(string title, BarColor color, BarStyle style, BarFlag flags = BarFlag.None)
    {
        var buf = System.Text.Encoding.UTF8.GetBytes(title + "\0");
        fixed (byte* p = buf)
        {
            var bar = T->ServerCreateBossBar(_ptr, p, (int)color, (int)style, (int)flags);
            return bar == null ? null : new BossBar(bar);
        }
    }
}
