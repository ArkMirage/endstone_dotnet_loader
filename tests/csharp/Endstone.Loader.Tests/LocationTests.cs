using System.Numerics;

namespace Endstone.Loader.Tests;

[TestClass]
public class LocationTests
{
    // ---- construction & basic accessors ----

    [TestMethod]
    public void Constructor_DefaultsPitchYawAndDimension()
    {
        var location = new Location(1, 2, 3);

        Assert.AreEqual(1f, location.X);
        Assert.AreEqual(2f, location.Y);
        Assert.AreEqual(3f, location.Z);
        Assert.AreEqual(0f, location.Pitch);
        Assert.AreEqual(0f, location.Yaw);
        Assert.IsNull(location.Dimension);
    }

    [TestMethod]
    public void Constructor_KeepsAllComponents()
    {
        var location = new Location(1, 2, 3, 4, 5);

        Assert.AreEqual(4f, location.Pitch);
        Assert.AreEqual(5f, location.Yaw);
    }

    // ---- block coordinates: must floor, matching the native static_cast<int>(std::floor(...)) ----

    [TestMethod]
    [DataRow(0f, 0)]
    [DataRow(1.9f, 1)]
    [DataRow(-0.1f, -1)]
    [DataRow(-1.5f, -2)]
    [DataRow(-2f, -2)]
    public void GetBlockX_Floors(float x, int expected) =>
        Assert.AreEqual(expected, new Location(x, 0, 0).GetBlockX());

    [TestMethod]
    [DataRow(0f, 0)]
    [DataRow(63.99f, 63)]
    [DataRow(-0.1f, -1)]
    [DataRow(-64.5f, -65)]
    public void GetBlockY_Floors(float y, int expected) =>
        Assert.AreEqual(expected, new Location(0, y, 0).GetBlockY());

    [TestMethod]
    [DataRow(0f, 0)]
    [DataRow(7.5f, 7)]
    [DataRow(-7.5f, -8)]
    public void GetBlockZ_Floors(float z, int expected) =>
        Assert.AreEqual(expected, new Location(0, 0, z).GetBlockZ());

    // ---- length / lengthSquared ----

    [TestMethod]
    public void LengthSquared_IsSumOfSquares()
    {
        Assert.AreEqual(25f, new Location(3, 4, 0).LengthSquared);
        Assert.AreEqual(0f, new Location(0, 0, 0).LengthSquared);
    }

    [TestMethod]
    public void Length_IsSqrtOfLengthSquared()
    {
        Assert.AreEqual(5f, new Location(3, 4, 0).Length);
        Assert.AreEqual(13f, new Location(3, 4, 12).Length);
    }

    // ---- zero: position only, orientation and dimension preserved ----

    [TestMethod]
    public void Zero_ClearsPositionOnly()
    {
        var zeroed = new Location(3, 4, 5, 10, 20).Zero();

        Assert.AreEqual(0f, zeroed.X);
        Assert.AreEqual(0f, zeroed.Y);
        Assert.AreEqual(0f, zeroed.Z);
        Assert.AreEqual(10f, zeroed.Pitch);
        Assert.AreEqual(20f, zeroed.Yaw);
    }

    // ---- arithmetic: Location +/- Location, matching the native operator overloads ----

    [TestMethod]
    public void Add_Location_AddsPositionAndKeepsLeftOrientation()
    {
        var left = new Location(1, 2, 3, 10, 20);
        var sum = left + new Location(4, 6, 8, 99, 99);

        Assert.AreEqual(new Vector3(5, 8, 11), sum.Position);
        Assert.AreEqual(10f, sum.Pitch);
        Assert.AreEqual(20f, sum.Yaw);
    }

    [TestMethod]
    public void Subtract_Location_SubtractsPosition()
    {
        var diff = new Location(4, 6, 8) - new Location(1, 2, 3);

        Assert.AreEqual(new Vector3(3, 4, 5), diff.Position);
    }

    // ---- arithmetic: Location +/- Vector3 ----

    [TestMethod]
    public void Add_Vector_OffsetsPositionAndKeepsOrientation()
    {
        var result = new Location(1, 2, 3, 10, 20) + new Vector3(1, 1, 1);

        Assert.AreEqual(new Vector3(2, 3, 4), result.Position);
        Assert.AreEqual(10f, result.Pitch);
        Assert.AreEqual(20f, result.Yaw);
    }

    [TestMethod]
    public void Subtract_Vector_OffsetsPosition()
    {
        var result = new Location(1, 2, 3) - new Vector3(1, 1, 1);

        Assert.AreEqual(new Vector3(0, 1, 2), result.Position);
    }

    // ---- arithmetic: scalar ----

    [TestMethod]
    public void Multiply_Scalar_ScalesPositionOnly()
    {
        var scaled = new Location(1, 2, 3, 10, 20) * 2f;

        Assert.AreEqual(new Vector3(2, 4, 6), scaled.Position);
        Assert.AreEqual(10f, scaled.Pitch);
        Assert.AreEqual(20f, scaled.Yaw);
    }

    [TestMethod]
    public void Operators_DoNotMutateOperands()
    {
        var location = new Location(1, 2, 3, 4, 5);

        _ = location + new Location(1, 1, 1) + new Vector3(1, 1, 1);
        _ = location * 3f;

        Assert.AreEqual(new Vector3(1, 2, 3), location.Position);
        Assert.AreEqual(4f, location.Pitch);
        Assert.AreEqual(5f, location.Yaw);
    }

    // ---- distance: the native version asserts both locations share a dimension ----

    [TestMethod]
    public void Distance_BetweenLocationsWithoutDimension_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new Location(0, 0, 0).Distance(new Location(3, 4, 0)));
    }

    [TestMethod]
    public void DistanceSquared_BetweenLocationsWithoutDimension_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new Location(0, 0, 0).DistanceSquared(new Location(3, 4, 0)));
    }

    [TestMethod]
    public void Distance_NullOther_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Location(0, 0, 0).Distance(null!));
    }

    // ---- orientation maths ----

    [TestMethod]
    public void Direction_ForLevelForward_IsPositiveZ()
    {
        var direction = new Location(0, 0, 0, 0, 0).Direction;

        Assert.AreEqual(0f, direction.X, 1e-5f);
        Assert.AreEqual(0f, direction.Y, 1e-5f);
        Assert.AreEqual(1f, direction.Z, 1e-5f);
    }

    [TestMethod]
    public void Direction_ForYaw90_IsNegativeX()
    {
        var direction = new Location(0, 0, 0, 0, 90).Direction;

        Assert.AreEqual(-1f, direction.X, 1e-5f);
        Assert.AreEqual(0f, direction.Y, 1e-5f);
        Assert.AreEqual(0f, direction.Z, 1e-5f);
    }

    [TestMethod]
    public void Direction_ForStraightDown_IsNegativeY()
    {
        var direction = new Location(0, 0, 0, 90, 0).Direction;

        Assert.AreEqual(0f, direction.X, 1e-5f);
        Assert.AreEqual(-1f, direction.Y, 1e-5f);
        Assert.AreEqual(0f, direction.Z, 1e-5f);
    }

    [TestMethod]
    public void Direction_IsUnitLength()
    {
        var direction = new Location(0, 0, 0, 30, 120).Direction;

        Assert.AreEqual(1f, direction.Length(), 1e-5f);
    }

    [TestMethod]
    public void WithDirection_RoundTripsYawAndPitch()
    {
        foreach (var yaw in new[] { 0f, 45f, 90f, 180f, 270f, 359f })
        {
            foreach (var pitch in new[] { -89f, -45f, 0f, 45f, 89f })
            {
                var source = new Location(0, 0, 0, pitch, yaw);
                var roundTripped = source.WithDirection(source.Direction);

                Assert.AreEqual(Location.NormalizeYaw(yaw), Location.NormalizeYaw(roundTripped.Yaw), 1e-3f,
                    $"yaw mismatch for yaw={yaw}, pitch={pitch}");
                Assert.AreEqual(pitch, roundTripped.Pitch, 1e-3f,
                    $"pitch mismatch for yaw={yaw}, pitch={pitch}");
            }
        }
    }

    [TestMethod]
    public void WithDirection_ForVerticalVector_SetsPitchAndKeepsYaw()
    {
        var up = new Location(0, 0, 0, 0, 123).WithDirection(new Vector3(0, 1, 0));
        Assert.AreEqual(-90f, up.Pitch);
        Assert.AreEqual(123f, up.Yaw);

        var down = new Location(0, 0, 0, 0, 123).WithDirection(new Vector3(0, -1, 0));
        Assert.AreEqual(90f, down.Pitch);
    }

    // ---- normalizeYaw / normalizePitch ----

    [TestMethod]
    [DataRow(0f, 0f)]
    [DataRow(90f, 90f)]
    [DataRow(180f, -180f)]
    [DataRow(-180f, -180f)]
    [DataRow(270f, -90f)]
    [DataRow(360f, 0f)]
    [DataRow(450f, 90f)]
    [DataRow(-450f, -90f)]
    [DataRow(720f, 0f)]
    public void NormalizeYaw_WrapsToPlusMinus180(float yaw, float expected) =>
        Assert.AreEqual(expected, Location.NormalizeYaw(yaw), 1e-5f);

    [TestMethod]
    [DataRow(0f, 0f)]
    [DataRow(45f, 45f)]
    [DataRow(90f, 90f)]
    [DataRow(120f, 90f)]
    [DataRow(-120f, -90f)]
    [DataRow(89.9f, 89.9f)]
    public void NormalizePitch_ClampsToPlusMinus90(float pitch, float expected) =>
        Assert.AreEqual(expected, Location.NormalizePitch(pitch), 1e-5f);

    // ---- immutable setters ----

    [TestMethod]
    public void WithX_ReplacesXOnly()
    {
        var updated = new Location(1, 2, 3, 4, 5).WithX(9);

        Assert.AreEqual(9f, updated.X);
        Assert.AreEqual(2f, updated.Y);
        Assert.AreEqual(3f, updated.Z);
        Assert.AreEqual(4f, updated.Pitch);
        Assert.AreEqual(5f, updated.Yaw);
    }

    [TestMethod]
    public void WithY_ReplacesYOnly()
    {
        var updated = new Location(1, 2, 3).WithY(9);

        Assert.AreEqual(new Vector3(1, 9, 3), updated.Position);
    }

    [TestMethod]
    public void WithZ_ReplacesZOnly()
    {
        var updated = new Location(1, 2, 3).WithZ(9);

        Assert.AreEqual(new Vector3(1, 2, 9), updated.Position);
    }

    [TestMethod]
    public void WithPitch_ReplacesPitchOnly()
    {
        var updated = new Location(1, 2, 3, 4, 5).WithPitch(9);

        Assert.AreEqual(9f, updated.Pitch);
        Assert.AreEqual(5f, updated.Yaw);
        Assert.AreEqual(new Vector3(1, 2, 3), updated.Position);
    }

    [TestMethod]
    public void WithYaw_ReplacesYawOnly()
    {
        var updated = new Location(1, 2, 3, 4, 5).WithYaw(9);

        Assert.AreEqual(4f, updated.Pitch);
        Assert.AreEqual(9f, updated.Yaw);
    }

    [TestMethod]
    public void WithDimension_ReplacesDimensionOnly()
    {
        var updated = new Location(1, 2, 3, 4, 5).WithDimension(null);

        Assert.IsNull(updated.Dimension);
        Assert.AreEqual(new Vector3(1, 2, 3), updated.Position);
        Assert.AreEqual(4f, updated.Pitch);
        Assert.AreEqual(5f, updated.Yaw);
    }

    [TestMethod]
    public void WithPosition_ReplacesPositionOnly()
    {
        var updated = new Location(1, 2, 3, 4, 5).WithPosition(new Vector3(7, 8, 9));

        Assert.AreEqual(new Vector3(7, 8, 9), updated.Position);
        Assert.AreEqual(4f, updated.Pitch);
        Assert.AreEqual(5f, updated.Yaw);
    }

    // ---- GetBlock without a dimension ----

    [TestMethod]
    public void GetBlock_WithoutDimension_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new Location(1, 2, 3).GetBlock());
    }

    // ---- conversions ----

    [TestMethod]
    public void ImplicitConversion_ToVector3_DropsOrientation()
    {
        Vector3 converted = new Location(1, 2, 3, 45, 90);

        Assert.AreEqual(new Vector3(1, 2, 3), converted);
    }

    [TestMethod]
    public void Position_MatchesCoordinates()
    {
        Assert.AreEqual(new Vector3(1, 2, 3), new Location(1, 2, 3, 45, 90).Position);
    }

    // ---- equality: 1e-6 epsilon over all components plus the dimension ----

    [TestMethod]
    public void Equality_IsEpsilonBased()
    {
        var a = new Location(1, 2, 3, 4, 5);

        Assert.AreEqual(a, new Location(1 + 1e-7f, 2, 3, 4, 5));
        Assert.AreNotEqual(a, new Location(1.001f, 2, 3, 4, 5));
    }

    [TestMethod]
    public void Equality_ComparesPitchAndYaw()
    {
        Assert.AreNotEqual(new Location(1, 2, 3, 4, 5), new Location(1, 2, 3, 4, 6));
        Assert.AreNotEqual(new Location(1, 2, 3, 4, 5), new Location(1, 2, 3, 5, 5));
    }

    [TestMethod]
    public void Equality_ForLocationsWithoutDimension_UsesValues()
    {
        Assert.AreEqual(new Location(1, 2, 3), new Location(1, 2, 3));
        Assert.IsTrue(new Location(1, 2, 3) == new Location(1, 2, 3));
        Assert.IsFalse(new Location(1, 2, 3) != new Location(1, 2, 3));
    }

    [TestMethod]
    public void Equality_HandlesNullsAndSelf()
    {
        var location = new Location(1, 2, 3);

        Assert.AreEqual(location, location);
        Assert.IsFalse(location.Equals((Location?)null));
        Assert.IsFalse(location == null);
        Assert.IsTrue((Location?)null == null);
        Assert.IsTrue(location != null);
    }

    [TestMethod]
    public void GetHashCode_IsStableForEqualValues()
    {
        Assert.AreEqual(new Location(1, 2, 3, 4, 5).GetHashCode(), new Location(1, 2, 3, 4, 5).GetHashCode());
    }

    [TestMethod]
    public void ToString_ListsCoordinates()
    {
        Assert.AreEqual("(1, 2, 3)", new Location(1, 2, 3).ToString());
    }
}
