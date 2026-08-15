#!/usr/bin/env bash
# Linux x64 build: native (C++) + managed (C#).
# The toolchain is taken from the environment (CC/CXX, DOTNET_ROOT); nothing
# is discovered or defined here besides plain output paths.
#
# Output layout (mirrors %RELEASE_DIR%, zip archives ignored):
#   ${PWD}/artifacts/linux-x64/plugins/endstone_dotnet_loader.so
#   ${PWD}/artifacts/linux-x64/plugins/dotnet_loader/runtime/Endstone.Loader.*
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

RID="linux-x64"
NATIVE_BASE="${SCRIPT_DIR}/build/${RID}"
OUT_DIR="$(pwd)/artifacts/${RID}"

command -v cmake >/dev/null 2>&1 || { echo "error: cmake not found on PATH" >&2; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "error: dotnet SDK not found on PATH" >&2; exit 1; }
command -v ninja >/dev/null 2>&1 || { echo "error: ninja not found on PATH" >&2; exit 1; }

CC="${CC:-clang}"
CXX="${CXX:-clang++}"
command -v "$CC" >/dev/null 2>&1 || { echo "error: \$CC ($CC) not found; install clang 18+ or set CC/CXX" >&2; exit 1; }
command -v "$CXX" >/dev/null 2>&1 || { echo "error: \$CXX ($CXX) not found; install clang 18+ or set CC/CXX" >&2; exit 1; }
DOTNET_ROOT="${DOTNET_ROOT:-$(dirname "$(readlink -f "$(command -v dotnet)")")}"
export DOTNET_ROOT

echo "Using CC=$CC CXX=$CXX"

echo "[1/3] Native loader (CMake/Ninja, clang+libc++)"
CC="$CC" CXX="$CXX" cmake -S "${SCRIPT_DIR}" -B "${NATIVE_BASE}" -G Ninja \
    -DCMAKE_BUILD_TYPE=Release \
    -DFETCHCONTENT_UPDATES_DISCONNECTED=ON \
    "$@"
cmake --build "${NATIVE_BASE}"

echo "[2/3] Managed loader (dotnet publish, net10.0)"
rm -rf "${OUT_DIR}"
dotnet publish "${SCRIPT_DIR}/src/csharp/Endstone.Loader.csproj" -c Release \
    -o "${OUT_DIR}/plugins/dotnet_loader/runtime" \
    -p:DebugSymbols=false -p:DebugType=None

echo "[3/3] Stage native plugin"
mkdir -p "${OUT_DIR}/plugins"
cp "${NATIVE_BASE}/src/cpp/endstone_dotnet_loader.so" "${OUT_DIR}/plugins/"

echo
echo "[done] linux-x64 artifacts staged under ${OUT_DIR}"