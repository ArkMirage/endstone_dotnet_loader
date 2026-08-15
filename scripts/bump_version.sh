#!/usr/bin/env bash
# bump_version.sh X.Y.Z — updates the version in the C++ CMake projects and the
# C# project so all three stay in sync.
set -euo pipefail

NEW="${1:?usage: bump_version.sh X.Y.Z}"
if [[ ! "$NEW" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "error: invalid version '${NEW}' (expected X.Y.Z)" >&2
    exit 1
fi

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

sed -i "s/(dotnet_loader VERSION [0-9.]\+/(dotnet_loader VERSION ${NEW}/" \
    "${ROOT}/CMakeLists.txt" \
    "${ROOT}/src/cpp/CMakeLists.txt"
sed -i "s#<Version>[0-9.]\+</Version>#<Version>${NEW}</Version>#" \
    "${ROOT}/src/csharp/Endstone.Loader.csproj"

grep -q "(dotnet_loader VERSION ${NEW}" "${ROOT}/CMakeLists.txt"
grep -q "(dotnet_loader VERSION ${NEW}" "${ROOT}/src/cpp/CMakeLists.txt"
grep -q "<Version>${NEW}</Version>" "${ROOT}/src/csharp/Endstone.Loader.csproj"

echo "version bumped to ${NEW}"