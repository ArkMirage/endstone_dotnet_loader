#include "dotnet_host.h"

#include <algorithm>
#include <cstdlib>
#include <filesystem>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <Windows.h>
#else
#include <dlfcn.h>
#endif

#include <coreclr_delegates.h>
#include <hostfxr.h>

namespace fs = std::filesystem;

namespace dotnet_loader {

namespace {

#if defined(_WIN32)
// char_t is wchar_t on Windows; paths are converted to UTF-8 for logging.
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

std::wstring s2ws(const std::string &s)
{
    if (s.empty()) {
        return {};
    }
    const int size = ::MultiByteToWideChar(CP_UTF8, 0, s.c_str(), static_cast<int>(s.size()), nullptr, 0);
    std::wstring out(size, L'\0');
    ::MultiByteToWideChar(CP_UTF8, 0, s.c_str(), static_cast<int>(s.size()), out.data(), size);
    return out;
}

using LibHandle = HMODULE;
LibHandle loadLibrary(const std::string &path) { return ::LoadLibraryW(s2ws(path).c_str()); }
void *resolveSymbol(LibHandle lib, const char *name)
{
    return reinterpret_cast<void *>(::GetProcAddress(lib, name));
}
std::string getEnv(const char *name)
{
    wchar_t *value = nullptr;
    if (_wdupenv_s(&value, nullptr, s2ws(name).c_str()) != 0 || !value) {
        return {};
    }
    std::wstring w(value);
    free(value);
    return ws2s(w);
}
// Returns the char_t (wchar_t on Windows, char on Linux) representation of a
// UTF-8 path for passing to the hostfxr API.
std::wstring toNativeStr(const std::string &s) { return s2ws(s); }

constexpr const char *kHostfxrName = "hostfxr.dll";
constexpr const char *kNetHostName = "nethost.dll";
constexpr const char *kDefaultRoots[] = {
    "C:\\Program Files\\dotnet",
};
#else
using LibHandle = void *;
LibHandle loadLibrary(const std::string &path) { return ::dlopen(path.c_str(), RTLD_NOW | RTLD_LOCAL); }
void *resolveSymbol(LibHandle lib, const char *name) { return ::dlsym(lib, name); }
std::string getEnv(const char *name)
{
    const char *value = std::getenv(name);
    return value ? value : "";
}
const std::string &toNativeStr(const std::string &s) { return s; }

constexpr const char *kHostfxrName = "libhostfxr.so";
constexpr const char *kNetHostName = "libnethost.so";
constexpr const char *kDefaultRoots[] = {
    "/usr/share/dotnet",
    "/usr/lib/dotnet",
    "/usr/local/share/dotnet",
};
#endif

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
        std::string cur;
        for (char c : p.filename().string()) {
            if (c == '.') {
                out.push_back(std::atoi(cur.c_str()));
                cur.clear();
            }
            else {
                cur += c;
            }
        }
        out.push_back(std::atoi(cur.c_str()));
        return out;
    };
    return parts(a) > parts(b);
}

// Tries to resolve and load hostfxr. Priority:
//   1) ENDSTONE_DOTNET_PATH env var (explicitly supplied .NET root)
//   2) nethost on the library search path (process dir, PATH/LD_LIBRARY_PATH, ...)
//   3) DOTNET_ROOT / default .NET installation folders
std::string findHostfxr()
{
    auto scan_roots = [](const std::vector<std::string> &roots) -> std::string {
        for (const auto &root : roots) {
            auto fxr = fs::path(root) / "host" / "fxr";
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
                auto lib = version / kHostfxrName;
                if (fs::exists(lib, ec)) {
                    return lib.string();
                }
            }
        }
        return {};
    };

    // 1) ENDSTONE_DOTNET_PATH env var, highest priority
    std::vector<std::string> roots;
    const auto endstone_root = getEnv("ENDSTONE_DOTNET_PATH");
    if (!endstone_root.empty()) {
        roots.push_back(endstone_root);
    }
    if (auto path = scan_roots(roots); !path.empty()) {
        return path;
    }

    // 2) nethost on the library search path (process dir, PATH, runtime dir, ...)
    if (auto nethost = loadLibrary(kNetHostName)) {
        auto get_hostfxr_path = reinterpret_cast<get_hostfxr_path_fn>(resolveSymbol(nethost, "get_hostfxr_path"));
        if (get_hostfxr_path) {
            char_t buffer[1024];
            size_t len = 1024;
            if (get_hostfxr_path(buffer, &len, nullptr) == 0) {
#if defined(_WIN32)
                return ws2s(buffer);
#else
                return std::string(buffer);
#endif
            }
        }
    }

    // 3) scan known dotnet roots for host/fxr/<version>/hostfxr
    roots.clear();
    const auto dotnet_root = getEnv("DOTNET_ROOT");
    if (!dotnet_root.empty()) {
        roots.push_back(dotnet_root);
    }
    for (const auto *r : kDefaultRoots) {
        roots.emplace_back(r);
    }
    return scan_roots(roots);
}

void loadHostfxr(std::string &out_path)
{
    if (init_for_config) {
        return;
    }

    const auto path = findHostfxr();
    if (path.empty()) {
        throw std::runtime_error("Unable to locate hostfxr: no nethost found and no .NET installation detected.");
    }
    out_path = path;

    LibHandle lib = loadLibrary(path);
    if (!lib) {
        throw std::runtime_error("Failed to load hostfxr from " + path);
    }

    init_for_config = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(
        resolveSymbol(lib, "hostfxr_initialize_for_runtime_config"));
    get_delegate =
        reinterpret_cast<hostfxr_get_runtime_delegate_fn>(resolveSymbol(lib, "hostfxr_get_runtime_delegate"));
    close_fptr = reinterpret_cast<hostfxr_close_fn>(resolveSymbol(lib, "hostfxr_close"));

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

    std::string fxr_path;
    loadHostfxr(fxr_path);

    // TEMP DEBUG: print the .NET installation being used
    const auto fxr_dir = fs::path(fxr_path).parent_path();
    const auto dotnet_root = fxr_dir.parent_path().parent_path().parent_path();
    const auto debug_msg = "[dotnet-loader] using hostfxr: " + fxr_path +
                           " (dotnet root: " + dotnet_root.string() + ")";
    log_fn(nullptr, 2, debug_msg.c_str());

    const auto config_path = runtime_dir_ / "Endstone.Loader.runtimeconfig.json";
    const auto assembly_path = runtime_dir_ / "Endstone.Loader.dll";
    if (!exists(config_path) || !exists(assembly_path)) {
        throw std::runtime_error("Endstone.Loader.dll / runtimeconfig.json not found in " + runtime_dir_.string());
    }

    hostfxr_handle ctx = nullptr;
    int rc = init_for_config(toNativeStr(config_path.string()).c_str(), nullptr, &ctx);
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

    // char_t is wchar_t on Windows and char (UTF-8) on Linux.
    const auto type_name = toNativeStr("Endstone.Loader.Bootstrap, Endstone.Loader");
    auto bind = [&](const char_t *method, void **target) {
        int r = load_asm_and_get_fn(toNativeStr(assembly_path.string()).c_str(), type_name.c_str(), method,
                                    UNMANAGEDCALLERSONLY_METHOD, nullptr, target);
        if (r != 0 || !*target) {
            throw std::runtime_error("Failed to bind managed method, code: " + std::to_string(r));
        }
    };

    InitFn init_fn = nullptr;
    bind(toNativeStr("Init").c_str(), reinterpret_cast<void **>(&init_fn));
    bind(toNativeStr("LoadPlugin").c_str(), reinterpret_cast<void **>(&load_plugin));
    bind(toNativeStr("Attach").c_str(), reinterpret_cast<void **>(&attach));
    bind(toNativeStr("OnLoad").c_str(), reinterpret_cast<void **>(&on_load));
    bind(toNativeStr("OnEnable").c_str(), reinterpret_cast<void **>(&on_enable));
    bind(toNativeStr("OnDisable").c_str(), reinterpret_cast<void **>(&on_disable));
    bind(toNativeStr("Release").c_str(), reinterpret_cast<void **>(&release));
    bind(toNativeStr("SetServer").c_str(), reinterpret_cast<void **>(&set_server));
    bind(toNativeStr("DispatchEvent").c_str(), reinterpret_cast<void **>(&dispatch_event));
    bind(toNativeStr("DispatchCommand").c_str(), reinterpret_cast<void **>(&dispatch_command));
    bind(toNativeStr("QueryCommands").c_str(), reinterpret_cast<void **>(&query_commands));
    bind(toNativeStr("FormDispatch").c_str(), reinterpret_cast<void **>(&dispatch_form));
    bind(toNativeStr("MapRenderDispatch").c_str(), reinterpret_cast<void **>(&dispatch_map_render));
    bind(toNativeStr("TaskDispatch").c_str(), reinterpret_cast<void **>(&dispatch_task));

    if (int r = init_fn(log_fn, bridge_table); r != 0) {
        throw std::runtime_error("Managed Bootstrap.Init failed, code: " + std::to_string(r));
    }

    started_ = true;
}

}  // namespace dotnet_loader
