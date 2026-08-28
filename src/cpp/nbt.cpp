// NBT (de)serialization between Endstone's public endstone::nbt::Tag hierarchy
// and CBOR. This is an internal concern of the loader; the bridge only ever
// hands the managed side already-serialized CBOR products.
//
// CBOR is used because nlohmann (already a dependency) emits/parses it natively.
// The only ambiguities are that CBOR collapses every integer width and both
// float widths into single types, and that ByteArray and IntArray would both
// be plain byte strings. We resolve this by encoding the six numeric NBT types
// and IntArray as CBOR byte strings carrying a semantic tag (subtype) equal to
// the NBT type id; a plain (untagged) byte string means ByteArray.
// String/List/Compound use native CBOR. All multi-byte payloads are
// little-endian, matching Bedrock NBT.

#include "nbt.h"

#include <cstdint>
#include <cstring>
#include <stdexcept>
#include <type_traits>
#include <vector>

#include <endstone/nbt/tag.h>
#include <nlohmann/json.hpp>

namespace dotnet_loader {

namespace {

// Little-endian raw bytes for a fixed-width value.
template <typename T>
std::vector<std::uint8_t> toLeBytes(T value)
{
    using U = std::make_unsigned_t<T>;
    U u = static_cast<U>(value);
    std::vector<std::uint8_t> b(sizeof(T));
    for (std::size_t i = 0; i < sizeof(T); ++i) {
        b[i] = static_cast<std::uint8_t>((u >> (8 * i)) & 0xFF);
    }
    return b;
}

template <typename T>
T fromLeBytes(const std::uint8_t *p)
{
    using U = std::make_unsigned_t<T>;
    U u = 0;
    for (std::size_t i = 0; i < sizeof(T); ++i) {
        u |= static_cast<U>(p[i]) << (8 * i);
    }
    return static_cast<T>(u);
}

// Wire type ids. nlohmann's CBOR writer drops binary subtypes and its reader
// rejects semantic tags, so we cannot use CBOR tags to carry the NBT type.
// Instead we prepend the type id as the first byte of the byte string. These
// ids must match the managed NbtTagType enum (see NbtTagType.cs).
constexpr std::uint8_t kWireByte = 1;
constexpr std::uint8_t kWireShort = 2;
constexpr std::uint8_t kWireInt = 3;
constexpr std::uint8_t kWireLong = 4;
constexpr std::uint8_t kWireFloat = 5;
constexpr std::uint8_t kWireDouble = 6;
constexpr std::uint8_t kWireByteArray = 7;
constexpr std::uint8_t kWireIntArray = 11;

// Encodes a numeric/IntArray tag as a CBOR byte string whose first byte is the
// NBT wire type id, followed by the little-endian raw value bytes.
nlohmann::json numericBinary(std::uint8_t subtype, std::vector<std::uint8_t> bytes)
{
    bytes.insert(bytes.begin(), subtype);
    return nlohmann::json::binary(std::move(bytes));
}

nlohmann::json nbtToJson(const endstone::nbt::Tag &tag)
{
    using namespace endstone::nbt;
    using endstone::ByteTag;
    using endstone::ShortTag;
    using endstone::IntTag;
    using endstone::LongTag;
    using endstone::FloatTag;
    using endstone::DoubleTag;
    using endstone::StringTag;
    using endstone::ByteArrayTag;
    using endstone::IntArrayTag;
    using endstone::ListTag;
    using endstone::CompoundTag;
    switch (tag.type()) {
    case Type::End:
        return nullptr;
    case Type::Byte:
        return numericBinary(1, toLeBytes(tag.get<ByteTag>().value()));
    case Type::Short:
        return numericBinary(2, toLeBytes(tag.get<ShortTag>().value()));
    case Type::Int:
        return numericBinary(3, toLeBytes(tag.get<IntTag>().value()));
    case Type::Long:
        return numericBinary(4, toLeBytes(tag.get<LongTag>().value()));
    case Type::Float: {
        const float f = tag.get<FloatTag>().value();
        std::uint32_t bits;
        std::memcpy(&bits, &f, sizeof(bits));
        return numericBinary(5, toLeBytes(bits));
    }
    case Type::Double: {
        const double d = tag.get<DoubleTag>().value();
        std::uint64_t bits;
        std::memcpy(&bits, &d, sizeof(bits));
        return numericBinary(6, toLeBytes(bits));
    }
    case Type::String:
        return tag.get<StringTag>().value();
    case Type::ByteArray: {
        const auto &data = tag.get<ByteArrayTag>();
        std::vector<std::uint8_t> bytes{kWireByteArray};
        bytes.insert(bytes.end(), data.begin(), data.end());
        return nlohmann::json::binary(std::move(bytes));  // id-prefixed byte string
    }
    case Type::IntArray: {
        const auto &data = tag.get<IntArrayTag>();
        std::vector<std::uint8_t> bytes;
        bytes.reserve(data.size() * sizeof(std::int32_t));
        for (const std::int32_t v : data) {
            const auto le = toLeBytes(v);
            bytes.insert(bytes.end(), le.begin(), le.end());
        }
        return numericBinary(11, std::move(bytes));  // subtype 11 -> IntArray
    }
    case Type::List: {
        const auto &list = tag.get<ListTag>();
        nlohmann::json arr = nlohmann::json::array();
        for (const auto &el : list) {
            arr.push_back(nbtToJson(el));
        }
        return arr;
    }
    case Type::Compound: {
        const auto &comp = tag.get<CompoundTag>();
        nlohmann::json obj = nlohmann::json::object();
        for (const auto &[key, value] : comp) {
            obj[key] = nbtToJson(value);
        }
        return obj;
    }
    default:
        return nullptr;
    }
}

endstone::nbt::Tag jsonToNbt(const nlohmann::json &j)
{
    using namespace endstone::nbt;
    using endstone::ByteTag;
    using endstone::ShortTag;
    using endstone::IntTag;
    using endstone::LongTag;
    using endstone::FloatTag;
    using endstone::DoubleTag;
    using endstone::StringTag;
    using endstone::ByteArrayTag;
    using endstone::IntArrayTag;
    using endstone::ListTag;
    using endstone::CompoundTag;
    switch (j.type()) {
    case nlohmann::json::value_t::null:
        return {};
    case nlohmann::json::value_t::string:
        return StringTag{j.get<std::string>()};
    case nlohmann::json::value_t::array: {
        ListTag list;
        for (const auto &el : j) {
            list.emplace_back(jsonToNbt(el));
        }
        return list;
    }
    case nlohmann::json::value_t::object: {
        CompoundTag comp;
        for (auto it = j.begin(); it != j.end(); ++it) {
            comp.insert_or_assign(it.key(), jsonToNbt(it.value()));
        }
        return comp;
    }
    case nlohmann::json::value_t::binary: {
        const auto &bin = j.get_binary();
        if (bin.empty()) {
            return ByteArrayTag{};
        }
        // First byte is the wire type id; the rest is the LE payload.
        const std::uint8_t id = bin[0];
        const std::uint8_t *p = bin.data() + 1;
        const std::size_t n = bin.size() - 1;
        switch (id) {
        case kWireByte:
            return ByteTag{fromLeBytes<std::uint8_t>(p)};
        case kWireShort:
            return ShortTag{fromLeBytes<std::int16_t>(p)};
        case kWireInt:
            return IntTag{fromLeBytes<std::int32_t>(p)};
        case kWireLong:
            return LongTag{fromLeBytes<std::int64_t>(p)};
        case kWireFloat: {
            const std::uint32_t bits = fromLeBytes<std::uint32_t>(p);
            float f;
            std::memcpy(&f, &bits, sizeof(f));
            return FloatTag{f};
        }
        case kWireDouble: {
            const std::uint64_t bits = fromLeBytes<std::uint64_t>(p);
            double d;
            std::memcpy(&d, &bits, sizeof(d));
            return DoubleTag{d};
        }
        case kWireByteArray:
            return ByteArrayTag{std::vector<std::uint8_t>(p, p + n)};
        case kWireIntArray: {
            const auto count = n / sizeof(std::int32_t);
            std::vector<std::int32_t> vals(count);
            for (std::size_t i = 0; i < count; ++i) {
                vals[i] = fromLeBytes<std::int32_t>(p + i * sizeof(std::int32_t));
            }
            return IntArrayTag{vals};
        }
        default:
            // Unknown id: treat the whole buffer as a raw byte array.
            return ByteArrayTag{std::vector<std::uint8_t>(bin.begin(), bin.end())};
        }
    }
    default:
        throw std::runtime_error("nbt: unexpected CBOR value type");
    }
}

}  // namespace

const char *nbtSerialize(const endstone::nbt::Tag &tag, int *len)
{
    const auto bytes = nlohmann::json::to_cbor(nbtToJson(tag));
    auto *buf = new char[bytes.size()];
    std::memcpy(buf, bytes.data(), bytes.size());
    if (len) {
        *len = static_cast<int>(bytes.size());
    }
    return buf;
}

void nbtFreeBuffer(void *ptr)
{
    delete[] static_cast<char *>(ptr);
}

std::optional<endstone::nbt::Tag> nbtDeserialize(const void *data, int len)
{
    if (!data || len <= 0) {
        return std::nullopt;
    }
    try {
        const auto *first = static_cast<const std::uint8_t *>(data);
        const std::vector<std::uint8_t> bytes(first, first + len);
        const nlohmann::json j = nlohmann::json::from_cbor(bytes);
        return jsonToNbt(j);
    }
    catch (...) {
        return std::nullopt;
    }
}

}  // namespace dotnet_loader
