#include "dotnet_host.h"

#include <algorithm>
#include <cstdlib>
#include <filesystem>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <Windows.h>

#include <coreclr_delegates.h>
#include <hostfxr.h>

namespace fs = std::filesystem;

namespace dotnet_loader {

namespace {

std::string ws2s(const std::wstring &w)
{
    if (w.empty()) {
        return {};
    }
    const int size = ::WideCharToMultiByte(CP_UTF8, 0, w.c_str(), static_cast<int>(w.size()), nullptr, 0, nullptr, nullptr);
    std::string out(size, '\0');
    ::WideCharToMultiByte(CP_UTF8, 0, w.c_str(), static_cast<int>(w.size()), out.data(), size, nullptr, nullptr);
    return out;
}

using get_hostfxr_path_fn = int (*)(char_t *, size_t *, const struct get_hostfxr_parameters *);

hostfxr_initialize_for_runtime_config_fn init_for_config = nullptr;
hostfxr_get_runtime_delegate_fn get_delegate = nullptr;
hostfxr_close_fn close_fptr = nullptr;

// Numeric version comparison: "10.0.11" > "9.0.7", unlike lexicographic string
// comparison ("1..." < "9...").
bool versionNewer(const fs::path &a, const fs::path &b)
{
    auto parts = [](const fs::path &p) {
        std::vector<int> out;
        std::wstring cur;
        for (wchar_t c : p.filename().wstring()) {
            if (c == L'.') {
                out.push_back(_wtoi(cur.c_str()));
                cur.clear();
            }
            else {
                cur += c;
            }
        }
        out.push_back(_wtoi(cur.c_str()));
        return out;
    };
    return parts(a) > parts(b);
}

// Tries to resolve and load hostfxr.dll. Priority:
//   1) ENDSTONE_DOTNET_PATH env var (explicitly supplied .NET root)
//   2) nethost.dll on the DLL search path (process dir, PATH, runtime dir, ...)
//   3) DOTNET_ROOT / default .NET installation folders
std::wstring findHostfxrDll()
{
    auto scan_roots = [](const std::vector<std::wstring> &roots) -> std::wstring {
        for (const auto &root : roots) {
            auto fxr = fs::path(root) / L"host" / L"fxr";
            std::error_code ec;
            if (!fs::exists(fxr, ec)) {
                continue;
            }
            std::vector<fs::path> versions;
            for (const auto &entry : fs::directory_iterator(fxr, ec)) {
                if (entry.is_directory()) {
                    versions.push_back(entry.path());
                }
            }
            std::sort(versions.begin(), versions.end(), versionNewer);  // newest first
            for (const auto &version : versions) {
                auto dll = version / L"hostfxr.dll";
                if (fs::exists(dll, ec)) {
                    return dll.wstring();
                }
            }
        }
        return {};
    };

    // 1) ENDSTONE_DOTNET_PATH env var, highest priority
    std::vector<std::wstring> roots;
    wchar_t *endstone_root = nullptr;
    if (_wdupenv_s(&endstone_root, nullptr, L"ENDSTONE_DOTNET_PATH") == 0 && endstone_root) {
        roots.emplace_back(endstone_root);
        free(endstone_root);
    }
    if (auto path = scan_roots(roots); !path.empty()) {
        return path;
    }

    // 2) nethost.dll on the DLL search path (process dir, PATH, runtime dir, ...)
    if (HMODULE nethost = ::LoadLibraryW(L"nethost.dll")) {
        auto get_hostfxr_path = reinterpret_cast<get_hostfxr_path_fn>(
            ::GetProcAddress(nethost, "get_hostfxr_path"));
        if (get_hostfxr_path) {
            wchar_t buffer[1024];
            size_t len = 1024;
            if (get_hostfxr_path(buffer, &len, nullptr) == 0) {
                return buffer;
            }
        }
    }

    // 3) scan known dotnet roots for host\fxr\<version>\hostfxr.dll
    roots.clear();
    wchar_t *root_env = nullptr;
    if (_wdupenv_s(&root_env, nullptr, L"DOTNET_ROOT") == 0 && root_env) {
        roots.emplace_back(root_env);
        free(root_env);
    }
    static const wchar_t *kDefaultRoots[] = {
        L"C:\\Program Files\\dotnet",
        L"C:\\Program Files\\dotnet\\",
    };
    for (const auto *r : kDefaultRoots) {
        roots.emplace_back(r);
    }
    return scan_roots(roots);
}

void loadHostfxr()
{
    if (init_for_config) {
        return;
    }

    const auto path = findHostfxrDll();
    if (path.empty()) {
        throw std::runtime_error("Unable to locate hostfxr.dll: no nethost.dll found and no .NET installation detected.");
    }

    HMODULE lib = ::LoadLibraryW(path.c_str());
    if (!lib) {
        throw std::runtime_error("Failed to load hostfxr.dll from " + ws2s(path));
    }

    init_for_config = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(
        ::GetProcAddress(lib, "hostfxr_initialize_for_runtime_config"));
    get_delegate =
        reinterpret_cast<hostfxr_get_runtime_delegate_fn>(::GetProcAddress(lib, "hostfxr_get_runtime_delegate"));
    close_fptr = reinterpret_cast<hostfxr_close_fn>(::GetProcAddress(lib, "hostfxr_close"));

    if (!init_for_config || !get_delegate || !close_fptr) {
        throw std::runtime_error("Failed to resolve hostfxr exports");
    }
}

}  // namespace

DotNetHost::DotNetHost(std::filesystem::path runtime_dir) : runtime_dir_(std::move(runtime_dir)) {}

void DotNetHost::start(LogFn log_fn, const void *bridge_table)
{
    if (started_) {
        return;
    }

    loadHostfxr();

    const auto config_path = runtime_dir_ / L"Endstone.Loader.runtimeconfig.json";
    const auto assembly_path = runtime_dir_ / L"Endstone.Loader.dll";
    if (!exists(config_path) || !exists(assembly_path)) {
        throw std::runtime_error("Endstone.Loader.dll / runtimeconfig.json not found in " + runtime_dir_.string());
    }

    hostfxr_handle ctx = nullptr;
    int rc = init_for_config(config_path.c_str(), nullptr, &ctx);
    // 0 = Success, 1 = Success_HostAlreadyInitialized, 2 = Success_DifferentRuntimeProperties
    if (rc < 0 || rc > 2 || !ctx) {
        close_fptr(ctx);
        throw std::runtime_error("hostfxr_initialize_for_runtime_config failed, code: " + std::to_string(rc));
    }

    load_assembly_and_get_function_pointer_fn load_asm_and_get_fn = nullptr;
    rc = get_delegate(ctx, hdt_load_assembly_and_get_function_pointer,
                      reinterpret_cast<void **>(&load_asm_and_get_fn));
    close_fptr(ctx);
    if (rc != 0 || !load_asm_and_get_fn) {
        throw std::runtime_error("hostfxr_get_runtime_delegate failed, code: " + std::to_string(rc));
    }

    const char_t *type_name = L"Endstone.Loader.Bootstrap, Endstone.Loader";
    auto bind = [&](const char_t *method, void **target) {
        int r = load_asm_and_get_fn(assembly_path.c_str(), type_name, method, UNMANAGEDCALLERSONLY_METHOD, nullptr,
                                    target);
        if (r != 0 || !*target) {
            throw std::runtime_error("Failed to bind managed method, code: " + std::to_string(r));
        }
    };

    InitFn init_fn = nullptr;
    bind(L"Init", reinterpret_cast<void **>(&init_fn));
    bind(L"LoadPlugin", reinterpret_cast<void **>(&load_plugin));
    bind(L"Attach", reinterpret_cast<void **>(&attach));
    bind(L"OnLoad", reinterpret_cast<void **>(&on_load));
    bind(L"OnEnable", reinterpret_cast<void **>(&on_enable));
    bind(L"OnDisable", reinterpret_cast<void **>(&on_disable));
    bind(L"Release", reinterpret_cast<void **>(&release));
    bind(L"SetServer", reinterpret_cast<void **>(&set_server));
    bind(L"DispatchEvent", reinterpret_cast<void **>(&dispatch_event));
    bind(L"DispatchCommand", reinterpret_cast<void **>(&dispatch_command));
    bind(L"QueryCommands", reinterpret_cast<void **>(&query_commands));
    bind(L"FormDispatch", reinterpret_cast<void **>(&dispatch_form));
    bind(L"MapRenderDispatch", reinterpret_cast<void **>(&dispatch_map_render));
    bind(L"TaskDispatch", reinterpret_cast<void **>(&dispatch_task));

    if (int r = init_fn(log_fn, bridge_table); r != 0) {
        throw std::runtime_error("Managed Bootstrap.Init failed, code: " + std::to_string(r));
    }

    started_ = true;
}

}  // namespace dotnet_loader
