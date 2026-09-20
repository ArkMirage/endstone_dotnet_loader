using System.Numerics;

namespace Endstone.Loader.Tests;

/// <summary>
/// Covers the members added to <see cref="Vector3Extensions"/>. These must not change any
/// System.Numerics.Vector3 behaviour - they only fill in the operations the native
/// endstone::Vector provides and Vector3 lacks.
/// </summary>
[TestClass]
public class Vector3ExtensionsTests
{
    [TestMethod]
    public void Dot_MatchesVector3Dot() =>
        Assert.AreEqual(Vector3.Dot(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
            new Vector3(1, 2, 3).Dot(new Vector3(4, 5, 6)));

    [TestMethod]
    public void Distance_MatchesVector3Distance() =>
        Assert.AreEqual(Vector3.Distance(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
            new Vector3(1, 2, 3).Distance(new Vector3(4, 5, 6)));

    [TestMethod]
    public void DistanceSquared_MatchesVector3DistanceSquared() =>
        Assert.AreEqual(Vector3.DistanceSquared(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
            new Vector3(1, 2, 3).DistanceSquared(new Vector3(4, 5, 6)));

    [TestMethod]
    public void CrossProduct_MatchesVector3Cross() =>
        Assert.AreEqual(Vector3.Cross(Vector3.UnitX, Vector3.UnitY),
            Vector3.UnitX.CrossProduct(Vector3.UnitY));

    [TestMethod]
    public void Angle_BetweenPerpendicularAxes_IsHalfPi()
    {
        var angle = Vector3.UnitX.Angle(Vector3.UnitY);

        Assert.AreEqual(MathF.PI / 2f, angle, 1e-5f);
    }

    [TestMethod]
    public void Angle_BetweenParallelVectors_IsZero()
    {
        Assert.AreEqual(0f, new Vector3(2, 0, 0).Angle(new Vector3(5, 0, 0)), 1e-5f);
    }

    [TestMethod]
    public void Angle_BetweenOppositeVectors_IsPi()
    {
        Assert.AreEqual(MathF.PI, Vector3.UnitX.Angle(-Vector3.UnitX), 1e-5f);
    }

    [TestMethod]
    public void Midpoint_IsComponentWiseAverage()
    {
        var midpoint = new Vector3(1, 2, 3).Midpoint(new Vector3(3, 4, 5));

        Assert.AreEqual(new Vector3(2, 3, 4), midpoint);
    }

    [TestMethod]
    [DataRow(1.9f, 1)]
    [DataRow(-0.1f, -1)]
    [DataRow(-1.5f, -2)]
    [DataRow(-2f, -2)]
    public void GetBlockX_Floors(float x, int expected) =>
        Assert.AreEqual(expected, new Vector3(x, 0, 0).GetBlockX());

    [TestMethod]
    public void GetBlockY_Floors() =>
        Assert.AreEqual(64, new Vector3(0, 64.99f, 0).GetBlockY());

    [TestMethod]
    public void GetBlockZ_Floors() =>
        Assert.AreEqual(-8, new Vector3(0, 0, -7.5f).GetBlockZ());

    [TestMethod]
    public void WithX_ReplacesXAndLeavesOriginalUntouched()
    {
        var original = new Vector3(1, 2, 3);
        var updated = original.WithX(9);

        Assert.AreEqual(new Vector3(9, 2, 3), updated);
        Assert.AreEqual(new Vector3(1, 2, 3), original);
    }

    [TestMethod]
    public void WithY_ReplacesYOnly() =>
        Assert.AreEqual(new Vector3(1, 9, 3), new Vector3(1, 2, 3).WithY(9));

    [TestMethod]
    public void WithZ_ReplacesZOnly() =>
        Assert.AreEqual(new Vector3(1, 2, 9), new Vector3(1, 2, 3).WithZ(9));

    [TestMethod]
    public void IsZero_DetectsAllZeroComponents()
    {
        Assert.IsTrue(Vector3.Zero.IsZero());
        Assert.IsFalse(new Vector3(0, 0, 1).IsZero());
    }

    [TestMethod]
    public void IsNormalized_DetectsUnitLength()
    {
        Assert.IsTrue(Vector3.UnitX.IsNormalized());
        Assert.IsTrue(Vector3.Normalize(new Vector3(3, 4, 0)).IsNormalized());
        Assert.IsFalse(new Vector3(1, 1, 0).IsNormalized());
    }

    [TestMethod]
    public void NormalizeZeros_ConvertsNegativeZero()
    {
        var normalized = new Vector3(-0.0f, 0.0f, -0.0f).NormalizeZeros();

        Assert.AreEqual(0.0f, normalized.X);
        Assert.AreEqual(0.0f, normalized.Y);
        Assert.AreEqual(0.0f, normalized.Z);

        // 1 / -0.0 is -Infinity, 1 / 0.0 is +Infinity: compare the bits to prove the sign flipped.
        Assert.IsTrue(float.IsPositiveInfinity(1f / normalized.X));
        Assert.IsTrue(float.IsPositiveInfinity(1f / normalized.Z));
    }

    [TestMethod]
    public void IsInAABB_ChecksInclusiveBounds()
    {
        var min = new Vector3(0, 0, 0);
        var max = new Vector3(2, 2, 2);

        Assert.IsTrue(new Vector3(1, 1, 1).IsInAABB(min, max));
        Assert.IsTrue(min.IsInAABB(min, max));
        Assert.IsTrue(max.IsInAABB(min, max));
        Assert.IsFalse(new Vector3(3, 1, 1).IsInAABB(min, max));
    }

    [TestMethod]
    public void IsInSphere_ChecksRadiusInclusively()
    {
        Assert.IsTrue(new Vector3(1, 1, 1).IsInSphere(Vector3.Zero, 2));
        Assert.IsTrue(new Vector3(2, 0, 0).IsInSphere(Vector3.Zero, 2));
        Assert.IsFalse(new Vector3(2.1f, 0, 0).IsInSphere(Vector3.Zero, 2));
    }

    [TestMethod]
    public void RotateAroundX_LeavesXUntouched()
    {
        var rotated = new Vector3(1, 1, 0).RotateAroundX(MathF.PI / 2f);

        Assert.AreEqual(1f, rotated.X, 1e-5f);
        Assert.AreEqual(0f, rotated.Y, 1e-5f);
        Assert.AreEqual(1f, rotated.Z, 1e-5f);
    }

    [TestMethod]
    public void RotateAroundY_LeavesYUntouched()
    {
        var rotated = new Vector3(1, 1, 0).RotateAroundY(MathF.PI / 2f);

        Assert.AreEqual(0f, rotated.X, 1e-5f);
        Assert.AreEqual(1f, rotated.Y, 1e-5f);
        Assert.AreEqual(-1f, rotated.Z, 1e-5f);
    }

    [TestMethod]
    public void RotateAroundZ_LeavesZUntouched()
    {
        var rotated = new Vector3(1, 0, 1).RotateAroundZ(MathF.PI / 2f);

        Assert.AreEqual(0f, rotated.X, 1e-5f);
        Assert.AreEqual(1f, rotated.Y, 1e-5f);
        Assert.AreEqual(1f, rotated.Z, 1e-5f);
    }

    [TestMethod]
    public void RotateAroundAxis_NormalizesTheAxisFirst()
    {
        var unitAxis = Vector3.UnitY.RotateAroundAxis(Vector3.UnitY, MathF.PI / 2f);
        var scaledAxis = Vector3.UnitY.RotateAroundAxis(Vector3.UnitY * 5f, MathF.PI / 2f);

        AssertVectorNear(new Vector3(0, 1, 0), unitAxis);
        AssertVectorNear(unitAxis, scaledAxis);
    }

    [TestMethod]
    public void RotateAroundNonUnitAxis_ScalesWithAxisLength()
    {
        var scaled = new Vector3(1, 0, 0).RotateAroundNonUnitAxis(Vector3.UnitY * 2f, MathF.PI / 2f);

        // A doubled axis doubles the resulting magnitude, matching the native behaviour.
        AssertVectorNear(new Vector3(0, 0, -2), scaled);
    }

    /// <summary>
    /// System.Numerics.Vector3 equality is exact, so rotated results need a tolerance.
    /// </summary>
    private static void AssertVectorNear(Vector3 expected, Vector3 actual, float delta = 1e-5f)
    {
        Assert.AreEqual(expected.X, actual.X, delta, "X");
        Assert.AreEqual(expected.Y, actual.Y, delta, "Y");
        Assert.AreEqual(expected.Z, actual.Z, delta, "Z");
    }

    [TestMethod]
    public void ToLocation_ZerosPitchAndYaw()
    {
        var location = new Vector3(1, 2, 3).ToLocation();

        Assert.AreEqual(new Vector3(1, 2, 3), location.Position);
        Assert.AreEqual(0f, location.Pitch);
        Assert.AreEqual(0f, location.Yaw);
    }
}
