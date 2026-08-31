# check_bridge.ps1
#
# Verifies that the C++ BridgeTable (src/cpp/include/bridge.h) and the C#
# Bridge.Table (src/csharp/Bridge.cs) stay in sync. Bridge.cs reads the table
# by field offset, so any mismatch (count, name, or order) silently breaks the
# ABI. This script checks all three:
#
#   1. Field count parity
#   2. Bidirectional name parity (compared in PascalCase canonical form)
#   3. Field order parity (the full sequence must match)
#
# Prints a final "RESULT: PASS" / "RESULT: FAIL" line. Always exits 0 — read
# the printed result, don't rely on the exit code.
#
# Usage:  powershell -File scripts/check_bridge.ps1
#         (run from anywhere; the repo root is derived from this script's path)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$hPath  = Join-Path $root 'src\cpp\include\bridge.h'
$csPath = Join-Path $root 'src\csharp\Bridge.cs'
$h  = Get-Content $hPath -Raw
$cs = Get-Content $csPath -Raw

# C++ field names: the identifier inside `(*name)(...)`.
$cpp = [regex]::Matches($h, '\(\*(\w+)\)') | ForEach-Object { $_.Groups[1].Value }
# C# field names: the identifier after the delegate* signature.
$csf = [regex]::Matches($cs, 'delegate\* unmanaged\[Cdecl\]<[^>]+> (\w+);') | ForEach-Object { $_.Groups[1].Value }

# snake_case -> PascalCase. Canonical form for BOTH directions of the name
# check, because the reverse (PascalCase -> snake_case) is ambiguous for
# acronyms (e.g. "YAt" could come from "y_at" or "yat").
function To-Pascal([string]$s) {
    ($s -split '_' | Where-Object { $_ -ne '' } | ForEach-Object { $_.Substring(0,1).ToUpper() + $_.Substring(1) }) -join ''
}

$fail = $false

# 1. Field count parity
if ($cpp.Count -ne $csf.Count) {
    $fail = $true
    Write-Host "FAIL count: bridge.h=$($cpp.Count)  Bridge.cs=$($csf.Count)" -ForegroundColor Red
} else {
    Write-Host "OK count: $($cpp.Count) fields on both sides" -ForegroundColor Green
}

# 2. Name parity (both directions compared in PascalCase canonical form)
$cppPascal = $cpp | ForEach-Object { To-Pascal $_ }
$missing = @($cppPascal | Where-Object { $_ -notin $csf })
$extra   = @($csf | Where-Object { $_ -notin $cppPascal })
if ($missing.Count -or $extra.Count) {
    $fail = $true
    Write-Host "FAIL name parity:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "  in bridge.h but not Bridge.cs: $_" -ForegroundColor Red }
    $extra   | ForEach-Object { Write-Host "  in Bridge.cs but not bridge.h: $_" -ForegroundColor Red }
} else {
    Write-Host "OK name parity: all $($csf.Count) names match bidirectionally" -ForegroundColor Green
}

# 3. Order parity — the full sequence must match, because Bridge.cs reads by
#    offset. Only run when both sides have the same count (otherwise the
#    comparison is meaningless).
if ($cpp.Count -eq $csf.Count) {
    $orderMismatch = $false
    for ($i = 0; $i -lt $cpp.Count; $i++) {
        if ($cppPascal[$i] -ne $csf[$i]) {
            if (-not $orderMismatch) {
                $orderMismatch = $true
                $fail = $true
                Write-Host "FAIL order parity (first mismatch at index $i):" -ForegroundColor Red
            }
            Write-Host "  [$i] bridge.h=$($cppPascal[$i])  Bridge.cs=$($csf[$i])" -ForegroundColor Red
        }
    }
    if (-not $orderMismatch) {
        Write-Host "OK order: all $($cpp.Count) fields in the same relative position" -ForegroundColor Green
    }
}

if ($fail) { Write-Host "RESULT: FAIL" -ForegroundColor Red } else { Write-Host "RESULT: PASS" -ForegroundColor Green }
