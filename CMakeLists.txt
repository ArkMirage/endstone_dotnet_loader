cmake_minimum_required(VERSION 3.15)

project(dotnet_loader VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(ENDSTONE_API_VERSION "0.11" CACHE STRING "Endstone API version")

# Headers (hostfxr.h, coreclr_delegates.h, nethost.h) come from the .NET app-host pack
set(NETHOST_ROOT "C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Host.win-x64/9.0.18/runtimes/win-x64/native"
    CACHE PATH "Directory containing the .NET hosting headers")

include(FetchContent)
FetchContent_Declare(
        endstone
        GIT_REPOSITORY https://github.com/EndstoneMC/endstone.git
        GIT_TAG v${ENDSTONE_API_VERSION}
)
FetchContent_MakeAvailable(endstone)

string(TOUPPER ${PROJECT_NAME} PROJECT_NAME_UPPER)
configure_file(include/version.h.in include/version.h)

endstone_add_plugin(${PROJECT_NAME}
        src/plugin.cpp
        src/dotnet_host.cpp
        src/dotnet_plugin_loader.cpp
        src/bridge.cpp
)
target_include_directories(${PROJECT_NAME} PRIVATE
        include
        ${CMAKE_CURRENT_BINARY_DIR}/include
        ${NETHOST_ROOT}
)
# hostfxr/nethost are loaded dynamically at runtime; no import library needed
