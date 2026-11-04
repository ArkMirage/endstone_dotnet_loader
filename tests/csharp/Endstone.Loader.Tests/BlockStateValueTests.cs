namespace Endstone.Loader.Tests;

/// <summary>Covers the union-shaped <see cref="BlockStateValue"/> API that will
/// be replaced by a C# 15 union once the project targets .NET 11.</summary>
[TestClass]
public class BlockStateValueTests
{
    [TestMethod]
    public void Boolean_RoundTrips()
    {
        var value = new BlockStateValue(true);

        Assert.AreEqual(BlockStateValueKind.Boolean, value.Kind);
        Assert.IsTrue((bool)value.Value!);
        Assert.IsTrue(value.BoolValue);
        Assert.IsTrue(value.TryGetBoolean(out var result));
        Assert.IsTrue(result);
        Assert.IsFalse(value.TryGetString(out _));
        Assert.IsFalse(value.TryGetInt32(out _));
    }

    [TestMethod]
    public void String_RoundTrips()
    {
        var value = new BlockStateValue("birch");

        Assert.AreEqual(BlockStateValueKind.String, value.Kind);
        Assert.AreEqual("birch", value.Value);
        Assert.AreEqual("birch", value.StringValue);
        Assert.IsTrue(value.TryGetString(out var result));
        Assert.AreEqual("birch", result);
        Assert.IsFalse(value.TryGetBoolean(out _));
        Assert.IsFalse(value.TryGetInt32(out _));
    }

    [TestMethod]
    public void Integer_RoundTrips()
    {
        var value = new BlockStateValue(42);

        Assert.AreEqual(BlockStateValueKind.Integer, value.Kind);
        Assert.AreEqual(42, value.Value);
        Assert.AreEqual(42, value.IntValue);
        Assert.IsTrue(value.TryGetInt32(out var result));
        Assert.AreEqual(42, result);
        Assert.IsFalse(value.TryGetBoolean(out _));
        Assert.IsFalse(value.TryGetString(out _));
    }

    [TestMethod]
    public void DefaultValue_HasNoneKind()
    {
        var value = default(BlockStateValue);

        Assert.AreEqual(BlockStateValueKind.None, value.Kind);
        Assert.IsNull(value.Value);
        Assert.IsFalse(value.TryGetBoolean(out _));
        Assert.IsFalse(value.TryGetString(out _));
        Assert.IsFalse(value.TryGetInt32(out _));
    }

    [TestMethod]
    public void ImplicitConversions_CreateExpectedKinds()
    {
        BlockStateValue boolean = true;
        BlockStateValue text = "spruce";
        BlockStateValue number = 7;

        Assert.AreEqual(BlockStateValueKind.Boolean, boolean.Kind);
        Assert.AreEqual(BlockStateValueKind.String, text.Kind);
        Assert.AreEqual(BlockStateValueKind.Integer, number.Kind);
    }

    [TestMethod]
    public void WrongAccessor_Throws()
    {
        var value = new BlockStateValue(true);

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = value.StringValue);
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = value.IntValue);
    }
}
