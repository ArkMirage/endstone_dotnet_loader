using System.Text.Json;

namespace Endstone.Loader;

/// <summary>Wraps a native endstone::BlockData. Instances returned by
/// <see cref="Block.GetData"/> own the native object and must be disposed.</summary>
public sealed unsafe class BlockData : IDisposable
{
    private void* _ptr;
    private readonly bool _ownsPtr;

    internal BlockData(IntPtr ptr, bool ownsPtr = true)
    {
        _ptr = (void*)ptr;
        _ownsPtr = ownsPtr;
    }

    internal IntPtr NativePtr => (IntPtr)_ptr;

    private static Bridge.Table* T => Bridge.Raw;

    /// <summary>The block type represented by this data, e.g. "minecraft:oak_log".</summary>
    public string Type => Bridge.Str(T->BlockDataGetType(_ptr));

    /// <summary>The runtime ID for this block state.</summary>
    public uint RuntimeId => T->BlockDataGetRuntimeId(_ptr);

    /// <summary>All block states as bool, string or int values. The dictionary
    /// is a snapshot built on each access.</summary>
    public IReadOnlyDictionary<string, BlockStateValue> BlockStates
    {
        get
        {
            var json = Bridge.Str(T->BlockDataGetBlockStatesJson(_ptr));
            using var document = JsonDocument.Parse(json);
            var states = new Dictionary<string, BlockStateValue>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                states[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.True => new BlockStateValue(true),
                    JsonValueKind.False => new BlockStateValue(false),
                    JsonValueKind.String => new BlockStateValue(property.Value.GetString()!),
                    JsonValueKind.Number => new BlockStateValue(property.Value.GetInt32()),
                    _ => throw new InvalidDataException(
                        $"Unsupported block state value for '{property.Name}'."),
                };
            }
            return states;
        }
    }

    public void Dispose()
    {
        if (_ownsPtr && _ptr != null)
        {
            T->BlockDataDelete(_ptr);
            _ptr = null;
        }
    }

    public override string ToString() => $"{Type} ({RuntimeId})";
}
