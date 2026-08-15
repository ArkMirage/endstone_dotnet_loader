@echo off
setlocal EnableExtensions
rem ------------------------------------------------------------------
rem Windows x64 build: native (C++) + managed (C#).
rem Must be launched from a Developer PowerShell (VsDevCmd loaded):
rem the MSVC/SDK/LLVM/Ninja/CMake environment is inherited, nothing is
rem discovered or redefined here.
rem
rem Output layout (mirrors %RELEASE_DIR%, zip archives ignored):
rem   %CD%\artifacts\win-x64\plugins\endstone_dotnet_loader.dll
rem   %CD%\artifacts\win-x64\plugins\dotnet_loader\runtime\Endstone.Loader.*
rem ------------------------------------------------------------------

if not defined VCToolsInstallDir (
    echo [error] MSVC environment not detected - launch from "Developer PowerShell"
    echo         ^(which runs VsDevCmd automatically^) and try again.
    exit /b 1
)
where clang-cl >nul 2>nul || (echo [error] clang-cl not on PATH; install "C++ Clang tools for Windows" & exit /b 1)
where ninja >nul 2>nul || (echo [error] ninja not on PATH; install "CMake tools for Windows" & exit /b 1)
where cmake >nul 2>nul || (echo [error] cmake not on PATH; install "CMake tools for Windows" & exit /b 1)
where dotnet >nul 2>nul || (echo [error] dotnet SDK not found on PATH & exit /b 1)

set "RID=win-x64"
set "NATIVE_BASE=%~dp0build\%RID%"
set "OUT_DIR=%CD%\artifacts\%RID%"

set "CC=clang-cl"
set "CXX=clang-cl"

echo [1/3] Native loader ^(CMake/Ninja, clang-cl^)
cmake -S "%~dp0." -B "%NATIVE_BASE%" -G Ninja -DCMAKE_BUILD_TYPE=Release -DFETCHCONTENT_UPDATES_DISCONNECTED=ON
if errorlevel 1 exit /b 1
cmake --build "%NATIVE_BASE%"
if errorlevel 1 exit /b 1

echo [2/3] Managed loader ^(dotnet publish, net10.0^)
if exist "%OUT_DIR%" rmdir /s /q "%OUT_DIR%"
dotnet publish "%~dp0src\csharp\Endstone.Loader.csproj" -c Release -o "%OUT_DIR%\plugins\dotnet_loader\runtime" -p:DebugSymbols=false -p:DebugType=None
if errorlevel 1 exit /b 1

echo [3/3] Stage native plugin
if not exist "%OUT_DIR%\plugins" mkdir "%OUT_DIR%\plugins"
copy /y "%NATIVE_BASE%\src\cpp\endstone_dotnet_loader.dll" "%OUT_DIR%\plugins\" >nul || (echo [error] native plugin missing & exit /b 1)

echo.
echo [done] win-x64 artifacts staged under %OUT_DIR%