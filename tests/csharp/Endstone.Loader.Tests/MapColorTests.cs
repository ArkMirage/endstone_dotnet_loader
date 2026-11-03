namespace Endstone.Loader.Tests;

/// <summary>
/// Covers the endstone::Color-compatible members on <see cref="MapColor"/>,
/// especially the BGR/ABGR conversions that do not exist on System.Drawing.Color.
/// </summary>
[TestClass]
public class MapColorTests
{
    [TestMethod]
    public void FromRgba_Integer_SplitsComponents() =>
        Assert.AreEqual(new MapColor(0x11, 0x22, 0x33, 0x44), MapColor.FromRgba(0x11223344));

    [TestMethod]
    public void FromRgb_Integer_SplitsComponents() =>
        Assert.AreEqual(new MapColor(0x11, 0x22, 0x33), MapColor.FromRgb(0x112233));

    [TestMethod]
    public void FromBgr_Integer_ReversesRedAndBlue() =>
        Assert.AreEqual(new MapColor(0x11, 0x22, 0x33), MapColor.FromBgr(0x332211));

    [TestMethod]
    public void FromBgr_Components_MatchesNativeOrder() =>
        Assert.AreEqual(new MapColor(0x11, 0x22, 0x33), MapColor.FromBgr(0x33, 0x22, 0x11));

    [TestMethod]
    public void FromAbgr_Integer_SplitsComponents() =>
        Assert.AreEqual(new MapColor(0x11, 0x22, 0x33, 0x44), MapColor.FromAbgr(0x44332211));

    [TestMethod]
    public void AsRgba_ReturnsPackedRgba() =>
        Assert.AreEqual(0x11223344, new MapColor(0x11, 0x22, 0x33, 0x44).AsRgba);

    [TestMethod]
    public void AsRgb_ReturnsPackedRgb() =>
        Assert.AreEqual(0x112233, new MapColor(0x11, 0x22, 0x33).AsRgb);

    [TestMethod]
    public void AsBgr_ReturnsPackedBgr() =>
        Assert.AreEqual(0x332211, new MapColor(0x11, 0x22, 0x33).AsBgr);

    [TestMethod]
    public void AsAbgr_ReturnsPackedAbgr() =>
        Assert.AreEqual(0x44332211, new MapColor(0x11, 0x22, 0x33, 0x44).AsAbgr);

    [TestMethod]
    public void WithMethods_ReplaceOnlyTheRequestedComponent()
    {
        var original = new MapColor(1, 2, 3, 4);

        Assert.AreEqual(new MapColor(9, 2, 3, 4), original.WithRed(9));
        Assert.AreEqual(new MapColor(1, 9, 3, 4), original.WithGreen(9));
        Assert.AreEqual(new MapColor(1, 2, 9, 4), original.WithBlue(9));
        Assert.AreEqual(new MapColor(1, 2, 3, 9), original.WithAlpha(9));
        Assert.AreEqual(new MapColor(1, 2, 3, 4), original);
    }

    [TestMethod]
    public void WhiteAndBlack_AreOpaque()
    {
        Assert.AreEqual(new MapColor(255, 255, 255, 255), MapColor.White);
        Assert.AreEqual(new MapColor(0, 0, 0, 255), MapColor.Black);
    }
}
