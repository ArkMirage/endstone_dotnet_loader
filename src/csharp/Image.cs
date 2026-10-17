namespace Endstone.Loader;

/// <summary>Pixel layout of an <see cref="Image"/> (endstone::Image::Type).</summary>
public enum ImageType
{
    Invalid = 0,
    Grayscale = 1,
    RGB = 3,
    RGBA = 4,
}

/// <summary>
/// An RGBA image, mirroring endstone::Image. Purely managed: the native side
/// only ever needs the raw pixels, which are copied across in both directions,
/// so no native handle is involved.
///
/// <see cref="Data"/> is always <c>Width * Height * 4</c> bytes in row-major
/// order; <see cref="Depth"/> is how many of the four bytes per pixel are
/// meaningful for the image type (the native buffer is sized the same way,
/// regardless of type).
/// </summary>
public sealed class Image
{
    public ImageType Type { get; }
    public int Width { get; }
    public int Height { get; }
    public byte[] Data { get; }

    /// <summary>Number of bytes per pixel used by <see cref="Type"/>.</summary>
    public int Depth => DepthOf(Type);

    /// <summary>Creates an empty (transparent black) image.</summary>
    public Image(ImageType type, int width, int height)
        : this(type, width, height, null) { }

    /// <summary>
    /// Creates an image from pixel data in a byte array. Mirrors
    /// Image::fromArray, which rejects an invalid type and a buffer whose size
    /// does not match the dimensions; shorter input is rejected rather than
    /// silently zero-padded.
    /// </summary>
    public Image(ImageType type, int width, int height, byte[]? data)
    {
        var depth = DepthOf(type);
        if (depth == 0)
        {
            throw new ArgumentException($"invalid image type: {type}", nameof(type));
        }
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "image dimensions must be positive");
        }

        var expected = width * height * depth;
        if (data != null && data.Length != expected)
        {
            throw new ArgumentException($"expected {expected} bytes, got {data.Length}", nameof(data));
        }

        Type = type;
        Width = width;
        Height = height;
        // The native buffer is depth-independent, so a grayscale/RGB image also
        // gets 4 bytes per pixel, with the trailing bytes left transparent.
        Data = new byte[width * height * 4];
        data?.CopyTo(Data, 0);
    }

    /// <summary>Gets the color of a pixel (endstone::Image::getColor).</summary>
    public MapColor GetColor(int x, int y)
    {
        var idx = CheckIndex(x, y);
        return Type switch
        {
            ImageType.Grayscale => new MapColor(Data[idx], Data[idx], Data[idx]),
            ImageType.RGB => new MapColor(Data[idx], Data[idx + 1], Data[idx + 2]),
            ImageType.RGBA => new MapColor(Data[idx], Data[idx + 1], Data[idx + 2], Data[idx + 3]),
            _ => new MapColor(0, 0, 0),
        };
    }

    /// <summary>Sets the color of a pixel (endstone::Image::setColor).</summary>
    public void SetColor(int x, int y, MapColor color)
    {
        var idx = CheckIndex(x, y);
        switch (Type)
        {
            case ImageType.Grayscale:
                Data[idx] = color.R;
                break;
            case ImageType.RGB:
                Data[idx] = color.R;
                Data[idx + 1] = color.G;
                Data[idx + 2] = color.B;
                break;
            case ImageType.RGBA:
                Data[idx] = color.R;
                Data[idx + 1] = color.G;
                Data[idx + 2] = color.B;
                Data[idx + 3] = color.A;
                break;
        }
    }

    /// <summary>Copies a native image buffer into a new managed <see cref="Image"/>.
    /// <paramref name="depth"/> doubles as the type because the native Image
    /// exposes no type accessor.</summary>
    internal static unsafe Image FromNative(int depth, int width, int height, byte* data, int length)
    {
        var type = (ImageType)depth;
        if (width > 0 && height > 0 && data != null && length >= width * height * 4)
        {
            var image = new Image(type, width, height);
            new ReadOnlySpan<byte>(data, image.Data.Length).CopyTo(image.Data);
            return image;
        }
        // Native always passes consistent dimensions; fall back to an empty
        // RGBA image rather than throwing out of a property getter.
        var w = width > 0 ? width : 1;
        var h = height > 0 ? height : 1;
        return new Image(type == ImageType.Invalid ? ImageType.RGBA : type, w, h);
    }

    private static int DepthOf(ImageType type) => type switch
    {
        ImageType.Grayscale => 1,
        ImageType.RGB => 3,
        ImageType.RGBA => 4,
        _ => 0,
    };

    private int CheckIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            throw new ArgumentOutOfRangeException($"({x}, {y}) is outside the {Width}x{Height} image");
        }
        return (y * Width + x) * 4;
    }
}
