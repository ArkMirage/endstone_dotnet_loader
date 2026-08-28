using System.Buffers.Binary;
using System.Formats.Cbor;

namespace Endstone.Loader.Nbt;

// CBOR (de)serializer for the NBT wire format.
// Wire convention (must match C++ nbtToJson in nbt.cpp):
//   numeric types (Byte..Double) and IntArray -> CBOR byte string whose first
//     byte is the NBT type id (1..6, 11) followed by LE raw value bytes
//   ByteArray -> CBOR byte string whose first byte is the id 7, then the bytes
//   String    -> CBOR text string
//   List      -> CBOR array
//   Compound  -> CBOR map
// We embed the type id in-band because nlohmann/json (used on the C++ side)
// drops CBOR binary subtypes and rejects semantic tags.
internal static class NbtCbor
{
    public static byte[] Write(NbtTag tag)
    {
        var w = new CborWriter(CborConformanceMode.Lax, convertIndefiniteLengthEncodings: true);
        WriteTag(w, tag);
        return w.Encode();
    }

    public static NbtTag Read(byte[] bytes)
    {
        var r = new CborReader(bytes, CborConformanceMode.Lax);
        return ReadTag(r);
    }

    private static void WriteTag(CborWriter w, NbtTag tag)
    {
        switch (tag.Type) {
        case NbtTagType.Byte:
        case NbtTagType.Short:
        case NbtTagType.Int:
        case NbtTagType.Long:
        case NbtTagType.Float:
        case NbtTagType.Double:
        case NbtTagType.IntArray:
            WriteNumeric(w, tag);
            break;
        case NbtTagType.ByteArray: {
            var ba = (byte[])tag.Value!;
            var framed = new byte[ba.Length + 1];
            framed[0] = (byte)NbtTagType.ByteArray;  // id 7
            Buffer.BlockCopy(ba, 0, framed, 1, ba.Length);
            w.WriteByteString(framed);
            break;
        }
        case NbtTagType.String:
            w.WriteTextString((string)tag.Value!);
            break;
        case NbtTagType.List:
            w.WriteStartArray(null);
            foreach (var item in (NbtTag[])tag.Value!) {
                WriteTag(w, item);
            }
            w.WriteEndArray();
            break;
        case NbtTagType.Compound:
            w.WriteStartMap(null);
            foreach (var kv in (Dictionary<string, NbtTag>)tag.Value!) {
                w.WriteTextString(kv.Key);
                WriteTag(w, kv.Value);
            }
            w.WriteEndMap();
            break;
        default:
            throw new InvalidDataException($"nbt: cannot serialize tag type {tag.Type}");
        }
    }

    private static void WriteNumeric(CborWriter w, NbtTag tag)
    {
        byte[] payload;
        int tagId;
        switch (tag.Type) {
        case NbtTagType.Byte:
            payload = [(byte)tag.Value!];
            tagId = 1;
            break;
        case NbtTagType.Short:
            payload = new byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(payload, (short)tag.Value!);
            tagId = 2;
            break;
        case NbtTagType.Int:
            payload = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(payload, (int)tag.Value!);
            tagId = 3;
            break;
        case NbtTagType.Long:
            payload = new byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(payload, (long)tag.Value!);
            tagId = 4;
            break;
        case NbtTagType.Float:
            payload = new byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(payload, (float)tag.Value!);
            tagId = 5;
            break;
        case NbtTagType.Double:
            payload = new byte[8];
            BinaryPrimitives.WriteDoubleLittleEndian(payload, (double)tag.Value!);
            tagId = 6;
            break;
        case NbtTagType.IntArray:
            var arr = (int[])tag.Value!;
            payload = new byte[arr.Length * 4];
            Buffer.BlockCopy(arr, 0, payload, 0, payload.Length);
            tagId = 11;
            break;
        default:
            throw new InvalidDataException($"nbt: unexpected numeric type {tag.Type}");
        }
        // Prepend the NBT type id (nlohmann drops CBOR tags, so embed in-band).
        var framed = new byte[payload.Length + 1];
        framed[0] = (byte)tagId;
        Buffer.BlockCopy(payload, 0, framed, 1, payload.Length);
        w.WriteByteString(framed);
    }

    private static NbtTag ReadTag(CborReader r)
    {
        switch (r.PeekState()) {
        case CborReaderState.TextString:
            return (NbtTag)r.ReadTextString();
        case CborReaderState.StartArray: {
            r.ReadStartArray();
            var items = new List<NbtTag>();
            while (r.PeekState() != CborReaderState.EndArray) {
                items.Add(ReadTag(r));
            }
            r.ReadEndArray();
            return (NbtTag)items.ToArray();
        }
        case CborReaderState.StartMap: {
            r.ReadStartMap();
            var dict = new Dictionary<string, NbtTag>();
            while (r.PeekState() != CborReaderState.EndMap) {
                var key = r.ReadTextString();
                dict[key] = ReadTag(r);
            }
            r.ReadEndMap();
            return (NbtTag)dict;
        }
        case CborReaderState.ByteString: {
            var bytes = r.ReadByteString();
            if (bytes.Length == 0) {
                return (NbtTag)Array.Empty<byte>();
            }
            // First byte is the wire type id; the rest is the LE payload.
            int id = bytes[0];
            var payload = bytes.AsSpan(1).ToArray();
            switch (id) {
            case 1:
                return (NbtTag)payload[0];
            case 2:
                return (NbtTag)BinaryPrimitives.ReadInt16LittleEndian(payload);
            case 3:
                return (NbtTag)BinaryPrimitives.ReadInt32LittleEndian(payload);
            case 4:
                return (NbtTag)BinaryPrimitives.ReadInt64LittleEndian(payload);
            case 5:
                return (NbtTag)BinaryPrimitives.ReadSingleLittleEndian(payload);
            case 6:
                return (NbtTag)BinaryPrimitives.ReadDoubleLittleEndian(payload);
            case 7:
                return (NbtTag)payload;
            case 11:
                var ints = new int[payload.Length / 4];
                Buffer.BlockCopy(payload, 0, ints, 0, payload.Length);
                return (NbtTag)ints;
            default:
                return (NbtTag)bytes;
            }
        }
        default:
            throw new InvalidDataException($"nbt: unexpected CBOR state {r.PeekState()}");
        }
    }

}
