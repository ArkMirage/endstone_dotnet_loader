namespace Endstone.Loader;

public enum MapScale
{
    Closest = 0,
    Close = 1,
    Normal = 2,
    Far = 3,
    Farthest = 4
}

public enum MapCursorType
{
    Player = 0,
    Frame = 1,
    RedMarker = 2,
    BlueMarker = 3,
    TargetX = 4,
    TargetPoint = 5,
    PlayerOffMap = 6,
    SignMarker = 7,
    PinkMarker = 8,
    OrangeMarker = 9,
    YellowMarker = 10,
    CyanMarker = 11,
    GreenPoint = 12,
    PlayerOffLimits = 13,
    Mansion = 14,
    Monument = 15,
    VillageDesert = 17,
    VillagePlains = 18,
    VillageSavanna = 19,
    VillageSnowy = 20,
    VillageTaiga = 21,
    JungleTemple = 22,
    SwampHut = 23,
    TrialChambers = 24
}

/// <summary>RGBA color used for map canvas pixels.</summary>
public readonly record struct MapColor(byte R, byte G, byte B, byte A = 255)
{
    public int AsRgba => (R << 24) | (G << 16) | (B << 8) | A;

    public static MapColor FromRgba(int rgba) =>
        new((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)rgba);

    public static MapColor FromRgb(byte r, byte g, byte b) => new(r, g, b);
}

/// <summary>A marker shown on a map.</summary>
public sealed class MapCursor
{
    public sbyte X { get; set; }
    public sbyte Y { get; set; }
    public sbyte Direction { get; set; }
    public MapCursorType Type { get; set; } = MapCursorType.Player;
    public bool Visible { get; set; } = true;
    public string Caption { get; set; } = "";

    public MapCursor() { }

    public MapCursor(sbyte x, sbyte y, sbyte direction, MapCursorType type, bool visible = true, string caption = "")
    {
        X = x;
        Y = y;
        Direction = direction;
        Type = type;
        Visible = visible;
        Caption = caption;
    }
}

/// <summary>Wraps a native endstone::MapView (server-owned; never dispose).</summary>
public sealed unsafe class MapView
{
    private readonly void* _ptr;

    internal MapView(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public long Id => T->MapGetId(_ptr);
    public bool IsVirtual => T->MapIsVirtual(_ptr);
    public MapScale Scale
    {
        get => (MapScale)T->MapGetScale(_ptr);
        set => T->MapSetScale(_ptr, (int)value);
    }
    public int CenterX
    {
        get => T->MapGetCenterX(_ptr);
        set => T->MapSetCenterX(_ptr, value);
    }
    public int CenterZ
    {
        get => T->MapGetCenterZ(_ptr);
        set => T->MapSetCenterZ(_ptr, value);
    }
    public Dimension? Dimension
    {
        get
        {
            var d = T->MapGetDimension(_ptr);
            return d == null ? null : new Dimension((IntPtr)d);
        }
    }
    public void SetDimension(Dimension dimension) => T->MapSetDimension(_ptr, (void*)dimension.NativePtr);
    public bool IsUnlimitedTracking
    {
        get => T->MapIsUnlimitedTracking(_ptr);
        set => T->MapSetUnlimitedTracking(_ptr, value);
    }
    public bool IsLocked
    {
        get => T->MapIsLocked(_ptr);
        set => T->MapSetLocked(_ptr, value);
    }

    /// <summary>Gets the dotnet-provided renderers attached to this map.</summary>
    public MapRenderer[] Renderers
    {
        get
        {
            var count = T->MapGetRendererCount(_ptr);
            var result = new List<MapRenderer>();
            for (var i = 0; i < count; i++)
            {
                ulong id;
                if (T->MapGetRenderer(_ptr, i, &id) != 0 && MapRenderer.Find(id) is { } renderer)
                {
                    result.Add(renderer);
                }
            }
            return result.ToArray();
        }
    }

    /// <summary>Attaches a renderer. The renderer draws on the canvas whenever
    /// this map is rendered for a player.</summary>
    public void AddRenderer(MapRenderer renderer) => renderer.Attach(this);

    /// <summary>Detaches a renderer. Returns true if it was attached to this map.</summary>
    public bool RemoveRenderer(MapRenderer renderer) => renderer.Detach(this);
}

/// <summary>Base class for plugin map renderers. Subclass and override Render to
/// draw onto the canvas.</summary>
public abstract unsafe class MapRenderer
{
    private static readonly object RegistryLock = new();
    private static readonly Dictionary<ulong, MapRenderer> Registry = new();
    private static long _nextId = 1;

    private readonly bool _contextual;
    private ulong _rendererId;
    private IntPtr _holder;

    protected MapRenderer(bool contextual = false) => _contextual = contextual;

    private static Bridge.Table* T => Bridge.Raw;

    public bool IsContextual => _contextual;
    public MapView? Map { get; internal set; }

    /// <summary>Renders one frame of the map for the given player.</summary>
    public abstract void Render(MapView map, MapCanvas canvas, Player player);

    internal void Attach(MapView map)
    {
        if (_holder != IntPtr.Zero)
        {
            throw new InvalidOperationException("Renderer is already attached to a map.");
        }
        _rendererId = (ulong)Interlocked.Increment(ref _nextId);
        _holder = (IntPtr)T->MapRendererCreate(_contextual ? 1 : 0, _rendererId);
        lock (RegistryLock)
        {
            Registry[_rendererId] = this;
        }
        Map = map;
        T->MapAddRenderer((void*)map.NativePtr, (void*)_holder);
    }

    internal bool Detach(MapView map)
    {
        if (_holder == IntPtr.Zero)
        {
            return false;
        }
        var removed = T->MapRemoveRenderer((void*)map.NativePtr, (void*)_holder);
        T->MapRendererDestroy((void*)_holder);
        _holder = IntPtr.Zero;
        lock (RegistryLock)
        {
            Registry.Remove(_rendererId);
        }
        Map = null;
        return removed;
    }

    internal static MapRenderer? Find(ulong id)
    {
        lock (RegistryLock)
        {
            return Registry.GetValueOrDefault(id);
        }
    }
}

/// <summary>Wraps a native endstone::MapCanvas (valid only during a Render call).</summary>
public sealed unsafe class MapCanvas
{
    private readonly void* _ptr;

    internal MapCanvas(IntPtr ptr) => _ptr = (void*)ptr;
    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    public MapView Map => new((IntPtr)T->CanvasGetMapView(_ptr));

    public MapCursor[] GetCursors()
    {
        var count = T->CanvasGetCursorCount(_ptr);
        var result = new MapCursor[count];
        var rec = stackalloc sbyte[5];
        for (var i = 0; i < count; i++)
        {
            T->CanvasGetCursor(_ptr, i, rec);
            result[i] = new MapCursor(rec[0], rec[1], rec[2], (MapCursorType)rec[3], rec[4] != 0,
                                      Bridge.Str(T->CanvasGetCursorCaption(_ptr, i)));
        }
        return result;
    }

    public void SetCursors(IEnumerable<MapCursor> cursors)
    {
        var list = cursors.ToArray();
        var records = new sbyte[list.Length * 5];
        var handles = new System.Runtime.InteropServices.GCHandle[list.Length];
        try
        {
            var captions = stackalloc byte*[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                var c = list[i];
                records[i * 5] = c.X;
                records[i * 5 + 1] = c.Y;
                records[i * 5 + 2] = c.Direction;
                records[i * 5 + 3] = (sbyte)c.Type;
                records[i * 5 + 4] = (sbyte)(c.Visible ? 1 : 0);
                var bytes = Bridge.ToUtf8(c.Caption);
                handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(bytes, System.Runtime.InteropServices.GCHandleType.Pinned);
                captions[i] = (byte*)handles[i].AddrOfPinnedObject();
            }
            fixed (sbyte* r = records)
            {
                T->CanvasSetCursors(_ptr, r, list.Length, captions);
            }
        }
        finally
        {
            foreach (var h in handles)
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }
        }
    }

    public void SetPixelColor(int x, int y, MapColor color)
        => T->CanvasSetPixelColor(_ptr, x, y, color.R, color.G, color.B, color.A);

    public MapColor GetPixelColor(int x, int y) => MapColor.FromRgba(T->CanvasGetPixelColor(_ptr, x, y));

    public MapColor GetBasePixelColor(int x, int y) => MapColor.FromRgba(T->CanvasGetBasePixelColor(_ptr, x, y));

    public void SetPixel(int x, int y, uint color) => T->CanvasSetPixel(_ptr, x, y, color);

    public uint GetPixel(int x, int y) => T->CanvasGetPixel(_ptr, x, y);

    public uint GetBasePixel(int x, int y) => T->CanvasGetBasePixel(_ptr, x, y);
}