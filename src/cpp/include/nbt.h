#pragma once

#include <optional>

#include <endstone/nbt/tag.h>

namespace dotnet_loader {

// Internal NBT (de)serialization used by the bridge. These are NOT exposed to
// the managed side: the bridge hands C# already-serialized CBOR products
// (e.g. item_get_nbt / item_set_nbt) rather than raw Endstone NBT pointers.
// endstone::nbt::Tag (any type) never leaves C++; its lifetime is managed by
// RAII, so there is no heap ownership transfer for the tag itself.
//
// The serialized bytes ARE handed to C# as a raw pointer, but unlike the
// callback direction (C++ -> C#) the managed side is the *caller* here, so it
// can copy the bytes and free the buffer itself. We therefore heap-allocate
// (no thread-local buffer) and expose nbtFreeBuffer for the managed side to
// release it.

// Serializes any endstone::nbt::Tag to CBOR into a heap-allocated buffer.
// The caller should free the buffer with nbtFreeBuffer after use. Returns
// nullptr on failure. Writes the byte length to *len.
const char *nbtSerialize(const endstone::nbt::Tag &tag, int *len);

// Frees a buffer returned by nbtSerialize.
void nbtFreeBuffer(void *ptr);

// Deserializes CBOR bytes into the root endstone::nbt::Tag (any type).
// Returns std::nullopt on failure.
std::optional<endstone::nbt::Tag> nbtDeserialize(const void *data, int len);

}  // namespace dotnet_loader
