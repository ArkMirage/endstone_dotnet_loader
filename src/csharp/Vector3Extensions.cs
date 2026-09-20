using System.Numerics;

namespace Endstone.Loader;

/// <summary>
/// Extension members that mirror the operations of the native endstone::Vector
/// which System.Numerics.Vector3 does not provide out of the box, plus the
/// bidirectional Location/Vector3 conversion (pitch and yaw zeroed).
/// </summary>
public static class Vector3Extensions
{
    extension(Vector3 v)
    {
        /// <summary>Gets the dot product of this vector with another.</summary>
        public float Dot(Vector3 other) => Vector3.Dot(v, other);

        /// <summary>Gets the distance between this vector and another.</summary>
        public float Distance(Vector3 other) => Vector3.Distance(v, other);

        /// <summary>Gets the squared distance between this vector and another.</summary>
        public float DistanceSquared(Vector3 other) => Vector3.DistanceSquared(v, other);

        /// <summary>Gets the cross product of this vector with another.</summary>
        public Vector3 CrossProduct(Vector3 other) => Vector3.Cross(v, other);

        /// <summary>Gets the angle between this vector and another, in radians.</summary>
        public float Angle(Vector3 other)
        {
            var dot = Math.Clamp(v.Dot(other) / (v.Length() * other.Length()), -1f, 1f);
            return MathF.Acos(dot);
        }

        /// <summary>Gets the midpoint between this vector and another.</summary>
        public Vector3 Midpoint(Vector3 other) =>
            new((v.X + other.X) / 2, (v.Y + other.Y) / 2, (v.Z + other.Z) / 2);

        /// <summary>Gets the floored X component, i.e. the block coordinate containing this vector.</summary>
        public int GetBlockX() => (int)MathF.Floor(v.X);

        /// <summary>Gets the floored Y component, i.e. the block coordinate containing this vector.</summary>
        public int GetBlockY() => (int)MathF.Floor(v.Y);

        /// <summary>Gets the floored Z component, i.e. the block coordinate containing this vector.</summary>
        public int GetBlockZ() => (int)MathF.Floor(v.Z);

        /// <summary>Returns a copy of this vector with a new X component.</summary>
        public Vector3 WithX(float x) => new(x, v.Y, v.Z);

        /// <summary>Returns a copy of this vector with a new Y component.</summary>
        public Vector3 WithY(float y) => new(v.X, y, v.Z);

        /// <summary>Returns a copy of this vector with a new Z component.</summary>
        public Vector3 WithZ(float z) => new(v.X, v.Y, z);

        /// <summary>Returns whether every component of this vector is zero.</summary>
        public bool IsZero() => v.X == 0 && v.Y == 0 && v.Z == 0;

        /// <summary>Returns whether this vector is normalized (length ~= 1).</summary>
        public bool IsNormalized()
        {
            const float eps = 1e-6f;
            return MathF.Abs(v.LengthSquared() - 1) < eps;
        }

        /// <summary>Converts each component of value -0.0 to 0.0.</summary>
        public Vector3 NormalizeZeros()
        {
            var x = v.X == -0.0f ? 0.0f : v.X;
            var y = v.Y == -0.0f ? 0.0f : v.Y;
            var z = v.Z == -0.0f ? 0.0f : v.Z;
            return new Vector3(x, y, z);
        }

        /// <summary>Returns whether this vector is inside the axis-aligned bounding box.</summary>
        public bool IsInAABB(Vector3 min, Vector3 max) =>
            v.X >= min.X && v.X <= max.X && v.Y >= min.Y && v.Y <= max.Y && v.Z >= min.Z && v.Z <= max.Z;

        /// <summary>Returns whether this vector is within the sphere of the given origin and radius.</summary>
        public bool IsInSphere(Vector3 origin, float radius) =>
            (origin.X - v.X) * (origin.X - v.X) + (origin.Y - v.Y) * (origin.Y - v.Y) +
            (origin.Z - v.Z) * (origin.Z - v.Z) <= radius * radius;

        /// <summary>Rotates the vector around the x-axis by the given angle (radians).</summary>
        public Vector3 RotateAroundX(float angle)
        {
            var c = MathF.Cos(angle);
            var s = MathF.Sin(angle);
            return new Vector3(v.X, c * v.Y - s * v.Z, s * v.Y + c * v.Z);
        }

        /// <summary>Rotates the vector around the y-axis by the given angle (radians).</summary>
        public Vector3 RotateAroundY(float angle)
        {
            var c = MathF.Cos(angle);
            var s = MathF.Sin(angle);
            return new Vector3(c * v.X + s * v.Z, v.Y, -s * v.X + c * v.Z);
        }

        /// <summary>Rotates the vector around the z-axis by the given angle (radians).</summary>
        public Vector3 RotateAroundZ(float angle)
        {
            var c = MathF.Cos(angle);
            var s = MathF.Sin(angle);
            return new Vector3(c * v.X - s * v.Y, s * v.X + c * v.Y, v.Z);
        }

        /// <summary>Rotates the vector around the given axis (normalized automatically) by the angle (radians).</summary>
        public Vector3 RotateAroundAxis(Vector3 axis, float angle)
        {
            var a = axis.IsNormalized() ? axis : Vector3.Normalize(axis);
            return v.RotateAroundNonUnitAxis(a, angle);
        }

        /// <summary>Rotates the vector around the given (non-unit) axis by the angle (radians).</summary>
        public Vector3 RotateAroundNonUnitAxis(Vector3 axis, float angle)
        {
            var x = v.X;
            var y = v.Y;
            var z = v.Z;
            var x2 = axis.X;
            var y2 = axis.Y;
            var z2 = axis.Z;

            var cosTheta = MathF.Cos(angle);
            var sinTheta = MathF.Sin(angle);

            var dot = x * x2 + y * y2 + z * z2;
            var xPrime = x2 * dot * (1.0f - cosTheta) + x * cosTheta + (-z2 * y + y2 * z) * sinTheta;
            var yPrime = y2 * dot * (1.0f - cosTheta) + y * cosTheta + (z2 * x - x2 * z) * sinTheta;
            var zPrime = z2 * dot * (1.0f - cosTheta) + z * cosTheta + (-y2 * x + x2 * y) * sinTheta;

            return new Vector3(xPrime, yPrime, zPrime);
        }

        /// <summary>Converts this vector to a Location with pitch and yaw zeroed.</summary>
        public Location ToLocation() => new(v.X, v.Y, v.Z, 0, 0);
    }
}
