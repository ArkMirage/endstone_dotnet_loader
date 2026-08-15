cmake_minimum_required(VERSION 3.15)

project(dotnet_loader VERSION 0.1.1 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(ENDSTONE_API_VERSION "0.11" CACHE STRING "Endstone API version")

# Headers (hostfxr.h, coreclr_delegates.h, nethost.h) come from the .NET app-host pack
if(WIN32)
    set(DOTNET_HOST_RID "win-x64" CACHE STRING ".NET host runtime identifier")
    set(_DEFAULT_DOTNET_ROOT "C:/Program Files/dotnet")
else()
    set(DOTNET_HOST_RID "linux-x64" CACHE STRING ".NET host runtime identifier")
    set(_DEFAULT_DOTNET_ROOT "$ENV{DOTNET_ROOT}")
    if(NOT _DEFAULT_DOTNET_ROOT)
        if(EXISTS "/usr/lib/dotnet")
            set(_DEFAULT_DOTNET_ROOT "/usr/lib/dotnet")
        else()
            set(_DEFAULT_DOTNET_ROOT "/usr/share/dotnet")
        endif()
    endif()
endif()

set(NETHOST_ROOT ""
    CACHE PATH "Directory containing the .NET hosting headers (hostfxr.h, coreclr_delegates.h, nethost.h)")

if(NOT NETHOST_ROOT)
    # Auto-discover the newest app-host pack under the .NET root. Prefer the
    # exact RID pack; distro packages use their own RID (e.g. ubuntu.26.04-x64).
    set(_host_packs "${_DEFAULT_DOTNET_ROOT}/packs/Microsoft.NETCore.App.Host.${DOTNET_HOST_RID}")
    file(GLOB _distro_packs "${_DEFAULT_DOTNET_ROOT}/packs/Microsoft.NETCore.App.Host.*")
    list(APPEND _host_packs ${_distro_packs})
    foreach(_host_pack IN LISTS _host_packs)
        if(NOT EXISTS "${_host_pack}")
            continue()
        endif()
        file(GLOB _host_versions RELATIVE "${_host_pack}" "${_host_pack}/*")
        list(SORT _host_versions ORDER DESCENDING)
        foreach(_host_version IN LISTS _host_versions)
            set(_native_dir "${_host_pack}/${_host_version}/runtimes/${DOTNET_HOST_RID}/native")
            if(NOT EXISTS "${_native_dir}/hostfxr.h")
                file(GLOB _native_dirs "${_host_pack}/${_host_version}/runtimes/*/native")
                if(_native_dirs)
                    list(GET _native_dirs 0 _native_dir)
                endif()
            endif()
            if(EXISTS "${_native_dir}/hostfxr.h")
                set(NETHOST_ROOT "${_native_dir}")
                break()
            endif()
        endforeach()
        if(NETHOST_ROOT)
            break()
        endif()
    endforeach()
endif()

if(NOT NETHOST_ROOT OR NOT EXISTS "${NETHOST_ROOT}/hostfxr.h")
    message(FATAL_ERROR
        "NETHOST_ROOT does not point at a .NET app-host pack native dir (hostfxr.h missing). "
        "Install the .NET SDK, or pass -DNETHOST_ROOT=<native dir>.")
endif()

include(FetchContent)
FetchContent_Declare(
        endstone
        GIT_REPOSITORY https://github.com/EndstoneMC/endstone.git
        GIT_TAG v${ENDSTONE_API_VERSION}
        GIT_SHALLOW ON
)
FetchContent_MakeAvailable(endstone)

string(TOUPPER ${PROJECT_NAME} PROJECT_NAME_UPPER)
configure_file(include/version.h.in ${CMAKE_CURRENT_BINARY_DIR}/include/version.h)

endstone_add_plugin(${PROJECT_NAME}
        plugin.cpp
        dotnet_host.cpp
        dotnet_plugin_loader.cpp
        bridge.cpp
)
target_include_directories(${PROJECT_NAME} PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/include
        ${CMAKE_CURRENT_BINARY_DIR}/include
        ${NETHOST_ROOT}
)
if(NOT WIN32)
    # dlopen/dlsym for hostfxr/nethost
    target_link_libraries(${PROJECT_NAME} PRIVATE ${CMAKE_DL_LIBS})
endif()
# hostfxr/nethost are loaded dynamically at runtime; no import library needed
