@echo off
set "MSVC=C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Tools\MSVC\14.50.35717"
set "SDK=D:\Windows Kits\10"
set "SDKVER=10.0.26100.0"
set "LLVM=C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Tools\Llvm\x64\bin"
set "NINJA=C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja"
set "PATH=%LLVM%;%NINJA%;%MSVC%\bin\Hostx64\x64;%SDK%\bin\%SDKVER%\x64;%PATH%"
set "INCLUDE=%MSVC%\include;%SDK%\Include\%SDKVER%\ucrt;%SDK%\Include\%SDKVER%\um;%SDK%\Include\%SDKVER%\shared;%SDK%\Include\%SDKVER%\winrt"
set "LIB=%MSVC%\lib\x64;%SDK%\Lib\%SDKVER%\ucrt\x64;%SDK%\Lib\%SDKVER%\um\x64"
set "CC=clang-cl"
set "CXX=clang-cl"
where clang-cl || exit /b 1
cmake -B build -G Ninja -DCMAKE_BUILD_TYPE=Release -DFETCHCONTENT_UPDATES_DISCONNECTED=ON || exit /b 1
cmake --build build
