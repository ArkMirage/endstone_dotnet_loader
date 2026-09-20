using System.Numerics;

namespace Endstone.Loader;

/// <summary>
/// Represents a 3-dimensional location in a dimension within a level. This is a
/// direct port of the native endstone::Location (see endstone/level/location.h),
/// which is a class carrying a dimension reference.
/// </summary>
public sealed class Location
{
    public Location(float x, float y, float z, float pitch = 0, float yaw = 0, Dimension? dimension = null)
    {
        X = x;
        Y = y;
        Z = z;
        Pitch = pitch;
        Yaw = yaw;
        Dimension = dimension;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float Pitch { get; }
    public float Yaw { get; }

    /// <summary>Gets the dimension this location resides in, or null if unknown.</summary>
    public Dimension? Dimension { get; }

    public override string ToString() => $"({X}, {Y}, {Z})";

    public int GetBlockX() => (int)MathF.Floor(X);
    public int GetBlockY() => (int)MathF.Floor(Y);
    public int GetBlockZ() => (int)MathF.Floor(Z);

    /// <summary>
    /// Gets the block at this location. Mirrors the native Location::getBlock(), which
    /// delegates to Dimension::getBlockAt(Location) using the floored coordinates.
    /// The caller owns the returned block (Dispose it).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This location carries no dimension, or the block could not be resolved.
    /// </exception>
    public Block GetBlock()
    {
        var dimension = Dimension ?? throw new InvalidOperationException(
            "This location has no dimension attached, so its block cannot be resolved.");
        return dimension.GetBlockAt(GetBlockX(), GetBlockY(), GetBlockZ())
               ?? throw new InvalidOperationException("Failed to get the block at this location.");
    }

    /// <summary>Gets the position of this location as a System.Numerics.Vector3.</summary>
    public Vector3 Position => new(X, Y, Z);

    /// <summary>Implicitly converts a location to a Vector3, dropping pitch, yaw and dimension.</summary>
    public static implicit operator Vector3(Location location) => new(location.X, location.Y, location.Z);

    /// <summary>Gets a unit-vector pointing in the direction this location faces.</summary>
    public Vector3 Direction
    {
        get
        {
            var rotX = Yaw * MathF.PI / 180f;
            var rotY = Pitch * MathF.PI / 180f;
            var xz = MathF.Cos(rotY);
            return new Vector3(-xz * MathF.Sin(rotX), -MathF.Sin(rotY), xz * MathF.Cos(rotX));
        }
    }

    /// <summary>Returns a copy of this location with the given position.</summary>
    public Location WithPosition(Vector3 position) => new(position.X, position.Y, position.Z, Pitch, Yaw, Dimension);

    /// <summary>Returns a copy of this location facing the given direction vector.</summary>
    public Location WithDirection(Vector3 direction)
    {
        var x = direction.X;
        var z = direction.Z;
        float yaw;
        float pitch;
        if (x == 0 && z == 0)
        {
            pitch = direction.Y > 0 ? -90 : 90;
            yaw = Yaw;
        }
        else
        {
            var theta = MathF.Atan2(-x, z);
            yaw = (theta + 2 * MathF.PI) % (2 * MathF.PI) * 180f / MathF.PI;
            var xz = MathF.Sqrt(x * x + z * z);
            pitch = MathF.Atan(-direction.Y / xz) * 180f / MathF.PI;
        }
        return new Location(X, Y, Z, pitch, yaw, Dimension);
    }

    /// <summary>Returns a copy of this location with the given X coordinate.</summary>
    public Location WithX(float x) => new(x, Y, Z, Pitch, Yaw, Dimension);

    /// <summary>Returns a copy of this location with the given Y coordinate.</summary>
    public Location WithY(float y) => new(X, y, Z, Pitch, Yaw, Dimension);

    /// <summary>Returns a copy of this location with the given Z coordinate.</summary>
    public Location WithZ(float z) => new(X, Y, z, Pitch, Yaw, Dimension);

    /// <summary>Returns a copy of this location with the given pitch, in degrees.</summary>
    public Location WithPitch(float pitch) => new(X, Y, Z, pitch, Yaw, Dimension);

    /// <summary>Returns a copy of this location with the given yaw, in degrees.</summary>
    public Location WithYaw(float yaw) => new(X, Y, Z, Pitch, yaw, Dimension);

    /// <summary>Returns a copy of this location in the given dimension.</summary>
    public Location WithDimension(Dimension? dimension) => new(X, Y, Z, Pitch, Yaw, dimension);

    /// <summary>
    /// Gets the magnitude of this location, defined as sqrt(x^2 + y^2 + z^2).
    /// Mirrors the native Location::length(); not world-aware and orientation independent.
    /// </summary>
    public float Length => MathF.Sqrt(LengthSquared);

    /// <summary>Gets the squared magnitude of this location, i.e. x^2 + y^2 + z^2.</summary>
    public float LengthSquared => (X * X) + (Y * Y) + (Z * Z);

    /// <summary>
    /// Gets the distance between this location and another. Mirrors the native
    /// Location::distance(), which is sqrt of distanceSquared().
    /// </summary>
    /// <exception cref="ArgumentException">The two locations are in different dimensions.</exception>
    public float Distance(Location other) => MathF.Sqrt(DistanceSquared(other));

    /// <summary>
    /// Gets the squared distance between this location and another. Mirrors the native
    /// Location::distanceSquared(), which requires both locations to share a dimension.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The two locations are in different dimensions, or either of them has no dimension.
    /// </exception>
    public float DistanceSquared(Location other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Dimension is null || other.Dimension is null || !SameDimensionAs(other))
        {
            throw new ArgumentException(
                $"Cannot measure distance between {Dimension?.Name ?? "<unknown>"} and " +
                $"{other.Dimension?.Name ?? "<unknown>"}.", nameof(other));
        }
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }

    /// <summary>
    /// Returns a copy of this location with its position zeroed, keeping pitch, yaw and
    /// dimension. Immutable equivalent of the native Location::zero().
    /// </summary>
    public Location Zero() => new(0, 0, 0, Pitch, Yaw, Dimension);

    /// <summary>
    /// Adds the position of another location, keeping this location's pitch, yaw and
    /// dimension. Mirrors the native Location::operator+=(const Location &amp;).
    /// </summary>
    public static Location operator +(Location left, Location right) =>
        left.WithPosition(left.Position + right.Position);

    /// <summary>
    /// Adds a vector to this location's position, keeping pitch, yaw and dimension.
    /// Mirrors the native Location::operator+=(const Vector &amp;).
    /// </summary>
    public static Location operator +(Location left, Vector3 right) =>
        left.WithPosition(left.Position + right);

    /// <summary>
    /// Subtracts the position of another location, keeping this location's pitch, yaw and
    /// dimension. Mirrors the native Location::operator-=(const Location &amp;).
    /// </summary>
    public static Location operator -(Location left, Location right) =>
        left.WithPosition(left.Position - right.Position);

    /// <summary>
    /// Subtracts a vector from this location's position, keeping pitch, yaw and dimension.
    /// Mirrors the native Location::operator-=(const Vector &amp;).
    /// </summary>
    public static Location operator -(Location left, Vector3 right) =>
        left.WithPosition(left.Position - right);

    /// <summary>
    /// Scales this location's position, keeping pitch, yaw and dimension.
    /// Mirrors the native Location::operator*=(double).
    /// </summary>
    public static Location operator *(Location location, float scalar) =>
        location.WithPosition(location.Position * scalar);

    /// <summary>
    /// Normalizes the given yaw angle to a value between +/-180 degrees.
    /// Mirrors the native Location::normalizeYaw().
    /// </summary>
    public static float NormalizeYaw(float yaw)
    {
        yaw %= 360.0f;
        if (yaw >= 180.0f)
        {
            yaw -= 360.0f;
        }
        else if (yaw < -180.0f)
        {
            yaw += 360.0f;
        }
        return yaw;
    }

    /// <summary>
    /// Normalizes the given pitch angle to a value between +/-90 degrees.
    /// Mirrors the native Location::normalizePitch().
    /// </summary>
    public static float NormalizePitch(float pitch) => Math.Clamp(pitch, -90.0f, 90.0f);

    /// <summary>
    /// Returns whether both locations reference the same native dimension. Dimensions are
    /// transient wrappers, so reference equality is not usable here - compare native handles.
    /// Two locations with no dimension attached are considered equal in this respect.
    /// </summary>
    private bool SameDimensionAs(Location other) =>
        Dimension?.NativePtr == other.Dimension?.NativePtr;

    public override bool Equals(object? obj) => obj is Location other && Equals(other);

    public bool Equals(Location? other)
    {
        if (other is null)
        {
            return false;
        }
        const float eps = 1e-6f;
        return SameDimensionAs(other) &&
               MathF.Abs(X - other.X) <= eps && MathF.Abs(Y - other.Y) <= eps &&
               MathF.Abs(Z - other.Z) <= eps && MathF.Abs(Pitch - other.Pitch) <= eps &&
               MathF.Abs(Yaw - other.Yaw) <= eps;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(X);
        hash.Add(Y);
        hash.Add(Z);
        hash.Add(Pitch);
        hash.Add(Yaw);
        hash.Add(Dimension?.NativePtr ?? IntPtr.Zero);
        return hash.ToHashCode();
    }

    public static bool operator ==(Location? a, Location? b) => a is null ? b is null : a.Equals(b);

    public static bool operator !=(Location? a, Location? b) => !(a == b);
}
