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

    public override bool Equals(object? obj) => obj is Location other && Equals(other);

    public bool Equals(Location? other)
    {
        if (other is null)
        {
            return false;
        }
        const float eps = 1e-6f;
        return Dimension == other.Dimension &&
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
        hash.Add(Dimension);
        return hash.ToHashCode();
    }

    public static bool operator ==(Location? a, Location? b) => a is null ? b is null : a.Equals(b);

    public static bool operator !=(Location? a, Location? b) => !(a == b);
}
