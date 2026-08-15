namespace Endstone.Loader;

/// <summary>Represents the priority of a service provider. Higher-priority
/// providers are returned by ServiceManager.Get first.</summary>
public enum ServicePriority
{
    Lowest = 0,
    Low = 1,
    Normal = 2,
    High = 3,
    Highest = 4,
}

/// <summary>
/// Base class for services registered with the server's service manager
/// (mirrors endstone::Service). A service is a marker interface: providers just
/// derive from this class and implement their own methods. The plugin must keep
/// its provider instance alive while it is registered (e.g. a field); the
/// native side only holds a shared_ptr proxy that forwards to this instance.
/// </summary>
public abstract unsafe class Service
{
    private static readonly object RegistryLock = new();
    // Weakly tracks dotnet-provided proxies so ServiceManager.Get can return
    // the original managed instance instead of a new wrapper. Weak references
    // keep the provider collectible while the owning plugin is still alive.
    private static readonly Dictionary<IntPtr, WeakReference> Registry = new();

    private readonly object _lock = new();
    private IntPtr _holder;
    private IntPtr _provider;

    protected Service() { }

    /// <summary>Wraps a provider returned by ServiceManager.Get. Takes ownership of the
    /// native holder (released when this wrapper is collected).</summary>
    private sealed class Wrapper : Service
    {
        internal Wrapper(IntPtr holder) : base(holder) { }
    }

    internal Service(IntPtr holder)
    {
        _holder = holder;
        _provider = (IntPtr)Bridge.Raw->ServiceProviderGetPtr((void*)holder);
    }

    /// <summary>Wraps a provider returned by ServiceManager.Get (takes ownership of the
    /// native holder, released when the wrapper is collected).</summary>
    internal static Service FromHolder(IntPtr holder) => new Wrapper(holder);

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>The native endstone::Service proxy pointer. Creates the proxy (and the
    /// owning shared_ptr holder) on first use.</summary>
    internal IntPtr ProviderPointer
    {
        get
        {
            if (_provider != IntPtr.Zero)
            {
                return _provider;
            }
            lock (_lock)
            {
                if (_provider == IntPtr.Zero)
                {
                    _holder = (IntPtr)T->ServiceProviderCreate();
                    _provider = (IntPtr)T->ServiceProviderGetPtr((void*)_holder);
                    lock (RegistryLock)
                    {
                        Registry[_provider] = new WeakReference(this);
                    }
                }
                return _provider;
            }
        }
    }

    internal static Service? Find(IntPtr provider)
    {
        lock (RegistryLock)
        {
            var reference = Registry.GetValueOrDefault(provider);
            return reference?.Target as Service;
        }
    }

    ~Service()
    {
        var holder = Interlocked.Exchange(ref _holder, IntPtr.Zero);
        if (holder == IntPtr.Zero)
        {
            return;
        }
        T->ServiceProviderRelease((void*)holder);
        var provider = Interlocked.Exchange(ref _provider, IntPtr.Zero);
        if (provider != IntPtr.Zero)
        {
            lock (RegistryLock)
            {
                Registry.Remove(provider);
            }
        }
    }
}

/// <summary>
/// Plugin-scoped facade over the server's native service manager
/// (mirrors endstone::ServiceManager). Registering a provider associates it
/// with this plugin; all registrations are dropped automatically when the
/// plugin is disabled.
/// </summary>
public sealed unsafe class ServiceManager
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _manager;
    private readonly void* _plugin;

    internal ServiceManager(IntPtr serverPtr, IntPtr pluginPtr)
    {
        _manager = T->ServerGetServiceManager((void*)serverPtr);
        _plugin = (void*)pluginPtr;
    }

    /// <summary>Registers a provider for the given service name.</summary>
    public void Register(string name, Service provider, ServicePriority priority = ServicePriority.Normal)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            T->ServiceManagerRegister(_manager, p, (void*)provider.ProviderPointer, _plugin, (int)priority);
        }
    }

    /// <summary>Unregisters all services registered by this plugin.</summary>
    public void UnregisterAll() => T->ServiceManagerUnregisterAll(_manager, _plugin);

    /// <summary>Unregisters a particular provider for a particular service.</summary>
    public void Unregister(string name, Service provider)
    {
        var buf = Bridge.ToUtf8(name);
        fixed (byte* p = buf)
        {
            T->ServiceManagerUnregister(_manager, p, (void*)provider.ProviderPointer);
        }
    }

    /// <summary>Unregisters a particular provider from every service it is registered for.</summary>
    public void Unregister(Service provider) => T->ServiceManagerUnregisterProvider(_manager, (void*)provider.ProviderPointer);

    /// <summary>Queries for a provider. Returns null if no provider has been registered for
    /// the service; otherwise the highest-priority provider is returned.</summary>
    public Service? Get(string name)
    {
        var buf = Bridge.ToUtf8(name);
        void* holder;
        fixed (byte* p = buf)
        {
            holder = T->ServiceManagerGet(_manager, p);
        }
        if (holder == null)
        {
            return null;
        }
        var provider = T->ServiceProviderGetPtr(holder);
        if (Service.Find(new IntPtr(provider)) is { } existing)
        {
            T->ServiceProviderRelease(holder);
            return existing;
        }
        return Service.FromHolder(new IntPtr(holder));
    }
}