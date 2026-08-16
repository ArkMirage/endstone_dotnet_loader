namespace Endstone.Loader;

/// <summary>
/// Possible default values for permissions.
/// Numeric values MUST stay aligned with the C++ endstone::PermissionDefault
/// declaration order (True=0, False=1, Operator=2, NotOperator=3, Console=4):
/// the value is bridged to the native side as a plain integer.
/// </summary>
public enum PermissionDefault
{
    True = 0,
    False = 1,
    Operator = 2,
    NotOperator = 3,
    Console = 4,
}

/// <summary>
/// The permission level of a Permissible (wraps endstone::PermissionLevel).
/// Numeric values MUST stay aligned with the C++ enum declaration order
/// (Default=0, Operator=1, Console=2): the value is bridged as an integer.
/// </summary>
public enum PermissionLevel
{
    Default = 0,
    Operator = 1,
    Console = 2,
}

/// <summary>
/// A permission that may be attached to a Permissible. Wraps a native
/// endstone::Permission and is used by the whole permission system: create
/// permissions with the plugin's <c>Permission(name)</c> factory (registered
/// automatically when the plugin is loaded) or with <c>new Permission(...)</c>
/// (register explicitly with Register()), configure them with the fluent
/// <c>With...</c> methods, look up registered ones with Permission.Get() and
/// check them via CommandSender.HasPermission.
///
/// Ownership: a wrapper created with the public constructor owns the native
/// object and frees it when collected (unless Register() transferred it to
/// the server's plugin manager). After Register() the wrapper becomes a
/// non-owning view of the plugin-manager-owned permission and stays fully
/// usable: keep it in a plugin field and keep using it to modify the
/// permission (children, parents, default, description) — the native side
/// keeps the same object alive, mirroring the C++ addPermission() reference.
/// Wrappers returned by Permission.Get() never own the native object (the
/// plugin manager does). Do not use a wrapper after the permission was
/// removed or the server reloaded plugins.
/// </summary>
public sealed unsafe class Permission
{
    private static Bridge.Table* T => Bridge.Raw;

    private void* _ptr;
    private bool _ownsNative;

    /// <summary>Creates a new permission. The caller owns the native object
    /// until Register() transfers it to the plugin manager; the native object
    /// is freed when this wrapper is collected if it was never registered.</summary>
    public Permission(string name, string description = "",
                      PermissionDefault defaultValue = PermissionDefault.Operator,
                      IReadOnlyDictionary<string, bool>? children = null)
    {
        var nameBuf = Bridge.ToUtf8(name);
        var descBuf = Bridge.ToUtf8(description);
        fixed (byte* pn = nameBuf)
        fixed (byte* pd = descBuf)
        {
            _ptr = T->PermissionCreate(pn, pd, (int)defaultValue);
        }
        _ownsNative = _ptr != null;
        if (children != null)
        {
            WithChildren(children);
        }
    }

    /// <summary>Wraps a plugin-manager-owned permission (non-owning).</summary>
    internal Permission(IntPtr ptr) => _ptr = (void*)ptr;

    /// <summary>Frees the native object when this wrapper still owns it
    /// (i.e. the permission was created but never registered).</summary>
    ~Permission()
    {
        if (_ownsNative && _ptr != null)
        {
            T->PermissionDestroy(_ptr);
        }
        _ptr = null;
        _ownsNative = false;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    /// <summary>Gets the unique fully qualified name of this permission.</summary>
    public string Name => Bridge.Str(T->PermissionGetName(_ptr));

    /// <summary>Gets or sets a brief description of this permission (may be empty).
    /// The change is temporary until the server reloads permissions.</summary>
    public string Description
    {
        get => Bridge.Str(T->PermissionGetDescription(_ptr));
        set => Bridge.Call1(T->PermissionSetDescription, _ptr, value);
    }

    /// <summary>Gets or sets the default value of this permission. Changing it
    /// recalculates all permissibles that contain this permission.</summary>
    public PermissionDefault Default
    {
        get => (PermissionDefault)T->PermissionGetDefault(_ptr);
        set => T->PermissionSetDefault(_ptr, (int)value);
    }

    /// <summary>Sets the description and returns this permission for chaining.</summary>
    public Permission WithDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>Sets the default value and returns this permission for chaining.</summary>
    public Permission WithDefault(PermissionDefault value)
    {
        Default = value;
        return this;
    }

    /// <summary>Adds or updates a child permission and returns this permission
    /// for chaining.</summary>
    public Permission WithChild(string name, bool value)
    {
        SetChild(name, value);
        return this;
    }

    /// <summary>Adds or updates multiple child permissions and returns this
    /// permission for chaining.</summary>
    public Permission WithChildren(IReadOnlyDictionary<string, bool> children)
    {
        foreach (var (child, value) in children)
        {
            SetChild(child, value);
        }
        return this;
    }

    /// <summary>Adds this permission to the named parent permission (created
    /// and registered if missing) and returns this permission for chaining.
    /// Requires the permission to be registered first, as the parent lookup
    /// goes through the plugin manager.</summary>
    public Permission WithParent(string name, bool value)
    {
        AddParent(name, value);
        return this;
    }

    /// <summary>Gets a snapshot of the children of this permission.</summary>
    public IReadOnlyDictionary<string, bool> Children
    {
        get
        {
            var result = new Dictionary<string, bool>();
            var count = T->PermissionGetChildCount(_ptr);
            for (var i = 0; i < count; i++)
            {
                var name = Bridge.Str(T->PermissionGetChildName(_ptr, i));
                result[name] = T->PermissionGetChildValue(_ptr, i);
            }
            return result;
        }
    }

    /// <summary>Adds or updates a child permission and recalculates all
    /// permissibles that contain this permission.</summary>
    public void SetChild(string name, bool value)
        => Bridge.CallVoidStrBool(T->PermissionSetChild, _ptr, name, value);

    /// <summary>Removes a child permission and recalculates all permissibles
    /// that contain this permission.</summary>
    public void RemoveChild(string name) => Bridge.Call1(T->PermissionRemoveChild, _ptr, name);

    /// <summary>Adds this permission to the specified parent permission. If the
    /// parent does not exist, it is created and registered. Returns the parent
    /// permission it created or loaded (non-owning).</summary>
    public Permission? AddParent(string name, bool value)
    {
        var parent = Bridge.CallPtrStrBool(T->PermissionAddParentName, _ptr, name, value);
        return parent == null ? null : new Permission((IntPtr)parent);
    }

    /// <summary>Adds this permission to the specified parent permission.</summary>
    public void AddParent(Permission parent, bool value)
        => T->PermissionAddParent(_ptr, (void*)parent.NativePtr, value);

    /// <summary>Recalculates all permissibles that contain this permission.
    /// Call this after modifying the children directly.</summary>
    public void RecalculatePermissibles() => T->PermissionRecalculate(_ptr);

    /// <summary>Registers this permission with the server's plugin manager.
    /// On success the plugin manager takes ownership of the native object and
    /// this wrapper becomes a non-owning view of the registered permission
    /// (the same native object, mirroring the C++ addPermission() reference).
    /// Keep the wrapper in a plugin field and keep using it to access the
    /// permission API. Returns false (ownership retained) when a permission
    /// with the same name is already registered. Idempotent: returns true if
    /// this permission is already registered.</summary>
    public bool Register()
    {
        ObjectDisposedException.ThrowIf(_ptr == null,typeof(Permission));
        if (!_ownsNative)
        {
            return true;  // already registered
        }
        var registered = T->PermissionAdd((void*)Bootstrap.ServerPtr, _ptr);
        if (registered == null)
        {
            return false;
        }
        _ptr = registered;
        _ownsNative = false;
        return true;
    }

    /// <summary>Removes this permission from the server's plugin manager (the
    /// native object is destroyed by the manager). Returns whether the
    /// permission was registered. The wrapper becomes invalid afterwards.</summary>
    public bool Remove()
    {
        if (_ptr == null)
        {
            return false;
        }
        var removed = Bridge.CallBoolStr(T->PermissionRemove, (void*)Bootstrap.ServerPtr, Name);
        if (removed)
        {
            _ptr = null;
            _ownsNative = false;
        }
        return removed;
    }

    /// <summary>Removes the named permission from the server's plugin manager.
    /// Returns whether the permission was registered.</summary>
    public static bool Remove(string name)
        => Bridge.CallBoolStr(T->PermissionRemove, (void*)Bootstrap.ServerPtr, name);

    /// <summary>Looks up a registered permission by its fully qualified name
    /// (case-insensitive). Returns null when no such permission is registered.
    /// The returned wrapper is a non-owning view of the plugin-manager-owned
    /// permission and can be used to access the permission API.</summary>
    public static Permission? Get(string name)
    {
        var p = Bridge.CallPtrStr(T->PermissionGet, (void*)Bootstrap.ServerPtr, name);
        return p == null ? null : new Permission((IntPtr)p);
    }

    public override string ToString() => Name;
}
