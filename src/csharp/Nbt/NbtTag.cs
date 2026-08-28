namespace Endstone.Loader.Nbt;


public enum NbtTagType : byte
{
    End = 0,
    Byte = 1,
    Short = 2,
    Int = 3,
    Long = 4,
    Float = 5,
    Double = 6,
    ByteArray = 7,
    String = 8,
    List = 9,
    Compound = 10,
    IntArray = 11,
}


// Value mapping:
//   Byte/Short/Int/Long -> byte/short/int/long
//   Float/Double        -> float/double
//   String              -> string
//   ByteArray           -> byte[]
//   IntArray            -> int[]
//   List                -> NbtTag[]
//   Compound            -> Dictionary<string, NbtTag>
// Still under progress: this desigen to be an union type, We are waiting for Dotnet 11
public sealed class NbtTag
{
    // Internal: only the implicit conversions below build a tag directly, so Type and
    // Value can never disagree. External callers (and NbtCbor) go through those
    // conversions. Will be removed when this migrates to a union.
    internal NbtTag(NbtTagType type, object? value)
    {
        Type = type;
        Value = value;
    }

    public NbtTagType Type { get; }
    public object? Value { get; }

    public static implicit operator NbtTag(byte value) => new(NbtTagType.Byte, value);
    public static implicit operator NbtTag(short value) => new(NbtTagType.Short, value);
    public static implicit operator NbtTag(int value) => new(NbtTagType.Int, value);
    public static implicit operator NbtTag(long value) => new(NbtTagType.Long, value);
    public static implicit operator NbtTag(float value) => new(NbtTagType.Float, value);
    public static implicit operator NbtTag(double value) => new(NbtTagType.Double, value);
    public static implicit operator NbtTag(string value) => new(NbtTagType.String, value);
    public static implicit operator NbtTag(byte[] value) => new(NbtTagType.ByteArray, value);
    public static implicit operator NbtTag(int[] value) => new(NbtTagType.IntArray, value);
    public static implicit operator NbtTag(NbtTag[] value) => new(NbtTagType.List, value);
    public static implicit operator NbtTag(Dictionary<string, NbtTag> value) => new(NbtTagType.Compound, value);

    // Recursively clones the tag tree into a fully independent copy. Scalars and strings
    // are immutable so they are re-wrapped as-is; arrays and containers are copied so the
    // clone shares no mutable state with the original. Call this before mutating a subtree
    // you don't own, to avoid alias traps.
    public NbtTag Clone()
    {
        return Type switch {
            NbtTagType.Byte      => (NbtTag)(byte)Value!,
            NbtTagType.Short     => (NbtTag)(short)Value!,
            NbtTagType.Int       => (NbtTag)(int)Value!,
            NbtTagType.Long      => (NbtTag)(long)Value!,
            NbtTagType.Float     => (NbtTag)(float)Value!,
            NbtTagType.Double    => (NbtTag)(double)Value!,
            NbtTagType.String    => (NbtTag)(string)Value!,
            NbtTagType.ByteArray => (NbtTag)(byte[])((byte[])Value!).Clone(),
            NbtTagType.IntArray  => (NbtTag)(int[])((int[])Value!).Clone(),
            NbtTagType.List      => (NbtTag)((NbtTag[])Value!).Select(t => t.Clone()).ToArray(),
            NbtTagType.Compound  => (NbtTag)((Dictionary<string, NbtTag>)Value!)
                .ToDictionary(kv => kv.Key, kv => kv.Value.Clone()),
            _ => throw new InvalidOperationException($"nbt: cannot clone tag type {Type}"),
        };
    }

    // Temporary debug helper: recursively renders the full tag tree. Deliberately
    // defensive so it never throws (returns a best-effort string on bad data).
    public override string ToString() => ToString(0);

    private string ToString(int depth)
    {
        try {
            var indent = new string(' ', depth * 2);
            return Type switch {
                NbtTagType.Compound => FormatCompound(indent, depth),
                NbtTagType.List => FormatList(indent, depth),
                NbtTagType.ByteArray => FormatByteArray(),
                NbtTagType.IntArray => FormatIntArray(),
                NbtTagType.String => $"{indent}String: \"{Value}\"",
                _ => $"{indent}{Type}: {Value}",
            };
        }
        catch {
            return $"<nbt:{Type} (unrenderable)>";
        }
    }

    private string FormatCompound(string indent, int depth)
    {
        if (Value is not Dictionary<string, NbtTag> dict) {
            return $"{indent}Compound: <null>";
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{indent}Compound ({dict.Count}):");
        foreach (var kv in dict) {
            sb.AppendLine($"{indent}  {kv.Key} =");
            sb.AppendLine(kv.Value?.ToString(depth + 2) ?? $"{new string(' ', (depth + 2) * 2)}<null>");
        }
        return sb.ToString().TrimEnd('\n', '\r');
    }

    private string FormatList(string indent, int depth)
    {
        if (Value is not NbtTag[] items) {
            return $"{indent}List: <null>";
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{indent}List ({items.Length}):");
        foreach (var item in items) {
            sb.AppendLine(item?.ToString(depth + 1) ?? $"{new string(' ', (depth + 1) * 2)}<null>");
        }
        return sb.ToString().TrimEnd('\n', '\r');
    }

    private string FormatByteArray()
    {
        if (Value is not byte[] b) {
            return $"ByteArray: <null>";
        }
        var preview = b.Length <= 16
            ? string.Join(" ", b)
            : $"{string.Join(" ", System.Linq.Enumerable.Take(b, 16))} ... (+{b.Length - 16} bytes)";
        return $"ByteArray[{b.Length}]: [{preview}]";
    }

    private string FormatIntArray()
    {
        if (Value is not int[] a) {
            return $"IntArray: <null>";
        }
        var preview = a.Length <= 16
            ? string.Join(" ", a)
            : $"{string.Join(" ", System.Linq.Enumerable.Take(a, 16))} ... (+{a.Length - 16} ints)";
        return $"IntArray[{a.Length}]: [{preview}]";
    }
}
