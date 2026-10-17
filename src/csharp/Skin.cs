namespace Endstone.Loader;

/// <summary>
/// A player skin (endstone::Skin). Skins are immutable and the native side
/// hands them out by value, so this wrapper always owns a heap-allocated copy:
/// Dispose (or collection) frees it. Reading a skin copies the whole object
/// including its images, so cache it instead of re-reading it in a loop.
/// </summary>
public sealed unsafe class Skin : IDisposable
{
    private void* _ptr;

    internal Skin(IntPtr ptr) => _ptr = (void*)ptr;

    private static Bridge.Table* T => Bridge.Raw;

    private void* Ptr => _ptr != null
        ? _ptr
        : throw new ObjectDisposedException(nameof(Skin));

    /// <summary>The skin id.</summary>
    public string Id => Bridge.Str(T->SkinGetId(Ptr));

    /// <summary>The cape id, or null when the skin has no cape.</summary>
    public string? CapeId
    {
        get
        {
            var p = T->SkinGetCapeId(Ptr);
            return p == null ? null : Bridge.Str(p);
        }
    }

    /// <summary>The skin image.</summary>
    public Image Image
    {
        get
        {
            var p = Ptr;
            int length = 0;
            var data = T->SkinGetImageData(p, &length);
            return Image.FromNative(T->SkinGetImageDepth(p), T->SkinGetImageWidth(p),
                                    T->SkinGetImageHeight(p), data, length);
        }
    }

    /// <summary>The cape image, or null when the skin has no cape.</summary>
    public Image? CapeImage
    {
        get
        {
            var p = Ptr;
            var depth = T->SkinGetCapeImageDepth(p);
            if (depth < 0)
            {
                return null;
            }
            int length = 0;
            var data = T->SkinGetCapeImageData(p, &length);
            return Image.FromNative(depth, T->SkinGetCapeImageWidth(p), T->SkinGetCapeImageHeight(p), data, length);
        }
    }

    /// <summary>Frees the native skin copy. A native Skin only holds strings and
    /// image buffers with no engine state or thread affinity, so releasing it
    /// from the finalizer thread is safe.</summary>
    public void Dispose()
    {
        if (_ptr != null)
        {
            T->SkinDelete(_ptr);
            _ptr = null;
        }
         GC.SuppressFinalize(this);
    }

    ~Skin() => Dispose();
}
