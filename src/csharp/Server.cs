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

    /// <summary>Gets or sets the maximum number of players allowed online.</summary>
    public int MaxPlayers
    {
        get => T->ServerGetMaxPlayers(_ptr);
        set => T->ServerSetMaxPlayers(_ptr, value);
    }

    /// <summary>Gets the game port the server runs on.</summary>
    public int Port => T->ServerGetPort(_ptr);

    /// <summary>Gets the IPv6 game port the server runs on.</summary>
    public int PortV6 => T->ServerGetPortV6(_ptr);

    /// <summary>Gets whether the server authenticates clients (online mode).</summary>
    public bool OnlineMode => T->ServerGetOnlineMode(_ptr);

    /// <summary>Gets whether the current thread is the primary server thread.</summary>
    public bool IsPrimaryThread => T->ServerIsPrimaryThread(_ptr);

    /// <summary>Gets the current milliseconds per tick (MSPT).</summary>
    public float CurrentMillisecondsPerTick => T->ServerGetCurrentMillisecondsPerTick(_ptr);

    /// <summary>Gets the average milliseconds per tick (MSPT).</summary>
    public float AverageMillisecondsPerTick => T->ServerGetAverageMillisecondsPerTick(_ptr);

    /// <summary>Gets the current ticks per second (TPS).</summary>
    public float CurrentTicksPerSecond => T->ServerGetCurrentTicksPerSecond(_ptr);

    /// <summary>Gets the average ticks per second (TPS).</summary>
    public float AverageTicksPerSecond => T->ServerGetAverageTicksPerSecond(_ptr);

    /// <summary>Gets the current tick usage as a percentage.</summary>
    public float CurrentTickUsage => T->ServerGetCurrentTickUsage(_ptr);

    /// <summary>Gets the average tick usage as a percentage.</summary>
    public float AverageTickUsage => T->ServerGetAverageTickUsage(_ptr);

    /// <summary>Gets the server start time (UTC).</summary>
    public DateTimeOffset StartTime => DateTimeOffset.FromUnixTimeMilliseconds(T->ServerGetStartTime(_ptr));

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

    /// <summary>Gets the player with the given UUID, or null if not online.</summary>
    public Player? GetPlayer(Guid uuid)
    {
        byte* buf = stackalloc byte[16];
        UuidMarshal.Write(uuid, new Span<byte>(buf, 16));
        var player = T->ServerGetPlayerByUuid(_ptr, buf);
        return player == null ? null : new Player((IntPtr)player);
    }

    /// <summary>Gets the console command sender.</summary>
    public ConsoleCommandSender ConsoleSender => new((IntPtr)T->ServerGetConsoleSender(_ptr));

    /// <summary>Gets the plugin manager, providing read-only access to all
    /// loaded plugins (both .NET and native).</summary>
    public PluginManager PluginManager => new((IntPtr)T->ServerGetPluginManager(_ptr));

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

    /// <summary>Gets the primary scoreboard controlled by the server (saved by
    /// the server and affected by the /scoreboard command), or null before the
    /// level has loaded. The returned wrapper is a non-owning view.</summary>
    public Scoreboard? GetScoreboard()
    {
        var board = T->ServerGetScoreboard(_ptr);
        return board == null ? null : new Scoreboard(board);
    }

    /// <summary>Creates a new scoreboard tracked by the server. It is not
    /// saved by the server and not affected by the /scoreboard command. The
    /// returned <see cref="OwnedScoreboard"/> owns the plugin's reference:
    /// Dispose it once the board is no longer assigned to any player
    /// (assign players back with Player.SetScoreboard(Server.GetScoreboard())
    /// or let them quit first).</summary>
    public OwnedScoreboard? CreateScoreboard()
    {
        var holder = T->ServerCreateScoreboard(_ptr);
        if (holder == null)
        {
            return null;
        }
        // The holder wraps a shared_ptr<Scoreboard>; read the raw pointer for
        // bridge calls and keep the holder for lifetime management.
        var board = T->ScoreboardHolderGet(holder);
        return board == null ? null : new OwnedScoreboard(board, holder);
    }

    /// <summary>Gets the player ban list (non-owning view; the server owns it).</summary>
    public PlayerBanList GetBanList() => new(T->ServerGetBanList(_ptr));

    /// <summary>Gets the IP ban list (non-owning view; the server owns it).</summary>
    public IpBanList GetIpBanList() => new(T->ServerGetIpBanList(_ptr));
}
