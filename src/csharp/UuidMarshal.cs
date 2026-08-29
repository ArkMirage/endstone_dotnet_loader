namespace Endstone.Loader;

/// <summary>
/// Marshals a <see cref="System.Guid"/> to and from the 16 raw bytes used to
/// pass UUIDs across the native bridge. The bytes are in canonical (network /
/// big-endian) order, matching <c>endstone::UUID::data</c> and Python's
/// <c>uuid.UUID.bytes</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="System.Guid"/> does not store its bytes in canonical order. Per
/// the .NET documentation, <c>Guid.ToByteArray()</c> returns the first
/// four-byte group and the next two two-byte groups in reversed
/// (little-endian) order, while the last two-byte and six-byte groups keep
/// their order — a "mixed-endian" (a.k.a. registry / Microsoft) layout. See
/// https://learn.microsoft.com/en-us/dotnet/api/system.guid.tobytearray.
/// </para>
/// <para>
/// The canonical UUID byte order (RFC 4122 §4.1.2) is fully big-endian. That is
/// exactly what <c>endstone::UUID::data</c> holds and what Python's
/// <c>uuid.UUID.bytes</c> returns ("the six integer fields in big-endian byte
/// order", https://docs.python.org/3/library/uuid.html).
/// </para>
/// <para>
/// Both .NET directions expose a <c>bigEndian</c> flag, so the conversion is
/// symmetric and needs no manual byte swapping:
/// <c>Guid.TryWriteBytes(destination, bigEndian: true, out _)</c> writes
/// canonical bytes, and <c>new Guid(source, bigEndian: true)</c> reads them
/// back. See
/// https://learn.microsoft.com/en-us/dotnet/api/system.guid.trywritebytes and
/// https://learn.microsoft.com/en-us/dotnet/api/system.guid.-ctor.
/// </para>
/// </remarks>
internal static class UuidMarshal
{
    /// <summary>
    /// Writes <paramref name="guid"/> as 16 canonical-order (big-endian) bytes.
    /// </summary>
    public static void Write(Guid guid, Span<byte> destination)
    {
        // bigEndian: true emits the canonical (RFC 4122) byte order shared with
        // the native side, so no manual reordering is needed.
        guid.TryWriteBytes(destination, bigEndian: true, out _);
    }

    /// <summary>
    /// Reads 16 canonical-order (big-endian) bytes into a <see cref="System.Guid"/>.
    /// </summary>
    public static Guid Read(ReadOnlySpan<byte> source)
    {
        // bigEndian: true interprets source as canonical (RFC 4122) bytes.
        return new Guid(source, bigEndian: true);
    }
}
