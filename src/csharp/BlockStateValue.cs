using System.Diagnostics.CodeAnalysis;

namespace Endstone.Loader;

/// <summary>Kind of a block state value.</summary>
public enum BlockStateValueKind
{
    None = 0,
    Boolean,
    String,
    Integer,
}

/// <summary>
/// A block state value, currently one of <see cref="bool"/>, <see cref="string"/>
/// or <see cref="int"/>. The public shape intentionally mirrors a future C# 15
/// union so it can be replaced by
/// <c>union BlockStateValue(bool, string, int)</c> without changing
/// <see cref="BlockData.BlockStates"/> or its consumers.
/// </summary>
public readonly struct BlockStateValue
{
    private readonly object? _value;

    public BlockStateValue(bool value) => _value = value;
    public BlockStateValue(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));
    public BlockStateValue(int value) => _value = value;

    /// <summary>The underlying value, or null for a default-constructed value.</summary>
    public object? Value => _value;

    public BlockStateValueKind Kind => _value switch
    {
        bool => BlockStateValueKind.Boolean,
        string => BlockStateValueKind.String,
        int => BlockStateValueKind.Integer,
        null => BlockStateValueKind.None,
        _ => throw new InvalidOperationException("Unsupported block state value type."),
    };

    public bool BoolValue => _value is bool value
        ? value
        : throw new InvalidOperationException("Block state value is not a boolean.");

    public string StringValue => _value is string value
        ? value
        : throw new InvalidOperationException("Block state value is not a string.");

    public int IntValue => _value is int value
        ? value
        : throw new InvalidOperationException("Block state value is not an integer.");

    public bool TryGetBoolean(out bool value)
    {
        if (_value is bool typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetString([NotNullWhen(true)] out string? value)
    {
        if (_value is string typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetInt32(out int value)
    {
        if (_value is int typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public static implicit operator BlockStateValue(bool value) => new(value);
    public static implicit operator BlockStateValue(string value) => new(value);
    public static implicit operator BlockStateValue(int value) => new(value);

    public override string ToString() => _value?.ToString() ?? string.Empty;
}
