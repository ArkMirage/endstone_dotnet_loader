#!/usr/bin/env bash
# Build the native loader on Linux.
# Prerequisites: clang (LLVM 18+) with libc++, cmake, ninja-build, and a .NET SDK
# installed under DOTNET_ROOT (default /usr/share/dotnet, /usr/lib/dotnet).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

# Locate the .NET root when DOTNET_ROOT is not set explicitly.
if [[ -z "${DOTNET_ROOT:-}" ]]; then
    if command -v dotnet >/dev/null 2>&1; then
        DOTNET_ROOT="$(dirname "$(readlink -f "$(command -v dotnet)")")"
    fi
fi
if [[ -z "${DOTNET_ROOT:-}" ]]; then
    echo "warning: dotnet not found on PATH; install the .NET SDK or set DOTNET_ROOT" >&2
fi
export DOTNET_ROOT

# Respect an explicit CC/CXX, otherwise fall back to "clang", then to the newest
# versioned clang (distros install clang-20 etc. without a plain "clang" symlink).
CC="${CC:-}"
if [[ -z "$CC" ]]; then
    if command -v clang >/dev/null 2>&1; then
        CC=clang
    else
        CC="$(command -v clang-22 clang-21 clang-20 clang-19 clang-18 2>/dev/null | head -n1 || true)"
    fi
fi
CXX="${CXX:-}"
if [[ -z "$CXX" ]]; then
    if command -v clang++ >/dev/null 2>&1; then
        CXX=clang++
    else
        CXX="$(command -v clang++-22 clang++-21 clang++-20 clang++-19 clang++-18 2>/dev/null | head -n1 || true)"
    fi
fi
if [[ -z "$CC" || -z "$CXX" ]]; then
    echo "error: clang (LLVM 18+) not found; install clang and libc++-dev" >&2
    exit 1
fi
echo "Using CC=$CC CXX=$CXX"

CC="$CC" CXX="$CXX" cmake -B build-linux -G Ninja \
    -DCMAKE_BUILD_TYPE=Release \
    -DFETCHCONTENT_UPDATES_DISCONNECTED=ON \
    "$@"
cmake --build build-linux
