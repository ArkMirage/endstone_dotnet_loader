namespace Endstone.Loader;

/// <summary>
/// Holds information about a permission attachment on a Permissible (wraps
/// endstone::PermissionAttachment). Created via CommandSender.AddAttachment;
/// the native object is owned by the permissible it is attached to, so this
/// wrapper is a non-owning view. The attachment stays valid until it is
/// removed or its permissible is destroyed.
/// </summary>
public sealed unsafe class PermissionAttachment
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal PermissionAttachment(IntPtr ptr) => _ptr = (void*)ptr;

    internal IntPtr NativePtr => (IntPtr)_ptr;

    /// <summary>Gets the plugin responsible for this attachment, or null if
    /// the plugin is no longer loaded.</summary>
    public Plugin? Plugin => Plugin.FromNative((IntPtr)T->AttachmentGetPlugin(_ptr));

    /// <summary>Gets the permissible this attachment is attached to.</summary>
    public CommandSender Permissible => new((IntPtr)T->AttachmentGetPermissible(_ptr));

    /// <summary>Gets a copy of all set permissions and values contained in
    /// this attachment. Modifying the returned dictionary does not affect the
    /// attachment (the native side returns a copy).</summary>
    public IReadOnlyDictionary<string, bool> Permissions
    {
        get
        {
            var count = T->AttachmentGetPermissionCount(_ptr);
            var result = new Dictionary<string, bool>(count);
            for (var i = 0; i < count; i++)
            {
                result[Bridge.Str(T->AttachmentGetPermissionName(_ptr, i))] = T->AttachmentGetPermissionValue(_ptr, i);
            }
            return result;
        }
    }

    /// <summary>Sets a permission to the given value, by fully qualified name.</summary>
    public void SetPermission(string name, bool value) => Bridge.CallVoidStrBool(T->AttachmentSetPermission, _ptr, name, value);

    /// <summary>Sets a permission to the given value.</summary>
    public void SetPermission(Permission permission, bool value) => T->AttachmentSetPermissionPerm(_ptr, (void*)permission.NativePtr, value);

    /// <summary>Removes the specified permission from this attachment, by
    /// fully qualified name.</summary>
    public void UnsetPermission(string name) => Bridge.Call1(T->AttachmentUnsetPermission, _ptr, name);

    /// <summary>Removes the specified permission from this attachment.</summary>
    public void UnsetPermission(Permission permission) => T->AttachmentUnsetPermissionPerm(_ptr, (void*)permission.NativePtr);

    /// <summary>Removes this attachment from its registered permissible.
    /// Returns true when removed successfully, false when it did not exist.</summary>
    public bool Remove() => T->AttachmentRemove(_ptr);
}

/// <summary>
/// Holds information on a permission and which PermissionAttachment provides
/// it (wraps endstone::PermissionAttachmentInfo). Returned by
/// CommandSender.GetEffectivePermissions(); the native objects are owned by
/// the permissible and remain valid until permissions are recalculated.
/// </summary>
public sealed unsafe class PermissionAttachmentInfo
{
    private static Bridge.Table* T => Bridge.Raw;

    private readonly void* _ptr;

    internal PermissionAttachmentInfo(IntPtr ptr) => _ptr = (void*)ptr;

    /// <summary>Gets the permissible this permission is for.</summary>
    public CommandSender Permissible => new((IntPtr)T->AttachmentInfoGetPermissible(_ptr));

    /// <summary>Gets the name of the permission.</summary>
    public string Permission => Bridge.Str(T->AttachmentInfoGetPermission(_ptr));

    /// <summary>Gets the attachment providing this permission, or null for
    /// default permissions (usually parent permissions).</summary>
    public PermissionAttachment? Attachment
    {
        get
        {
            var a = T->AttachmentInfoGetAttachment(_ptr);
            return a == null ? null : new PermissionAttachment((IntPtr)a);
        }
    }

    /// <summary>Gets the value of this permission.</summary>
    public bool Value => T->AttachmentInfoGetValue(_ptr);
}
