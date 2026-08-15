#pragma once

#include <cstdint>
#include <filesystem>

namespace dotnet_loader {

// Native callback given to managed code: log(plugin_native_handle, level, utf8_message).
// plugin_native_handle may be null (falls back to the loader's logger).
using LogFn = void (*)(void *plugin, int level, const char *message);

// Managed entry points (UnmanagedCallersOnly, Cdecl on x64 is the default calling convention)
using InitFn = int (*)(LogFn log_fn, const void *bridge_table);
// Loads plugin assembly, returns GCHandle (0 on failure) and writes a JSON
// plugin info object {"name","version","description","authors","commands"}
// into buffer (utf8); on failure writes a plain-text error message.
using LoadPluginFn = void *(*)(const char *assembly_path_utf8, char *info_buffer, int32_t buffer_size);
// Associates the managed plugin instance with its native proxy pointer (for logging).
using AttachFn = void (*)(void *gc_handle, void *native_plugin);
using LifecycleFn = void (*)(void *gc_handle);
// Hands the server pointer to managed code (Server is a singleton).
using SetServerFn = void (*)(void *server);
// Invoked by the native event handler; cb_handle is a GCHandle to Action<IntPtr>.
using DispatchEventFn = void (*)(void *gc_handle, void *cb_handle, void *event_ptr);
// Dispatches a plugin command to managed code; returns true when handled.
using DispatchCommandFn = int (*)(void *gc_handle, void *sender, const char *command_name,
                                  const char *const *args, int32_t arg_count);
// Re-queries the commands declared by a managed plugin (e.g. after OnLoad);
// writes a JSON array of command objects {"name","description","usages",
// "aliases","permissions"}; returns 1 on success, 0 when the handle is unknown.
using QueryCommandsFn = int (*)(void *gc_handle, char *buffer, int32_t buffer_size);
// Form submit/close dispatched back to managed code.
// result_kind: 0 = submit (button_index for message/action forms, 0 for modal),
// 1 = close (button_index = -1). payload is UTF-8 (empty for message/action forms,
// JSON for modal forms).
using DispatchFormFn = void (*)(void *player, int result_kind, uint64_t form_id, int button_index,
                                const char *payload);
// Map render callback: the native BridgeMapRenderer forwards endstone's
// render(map, canvas, player) call back to managed code.
using DispatchMapRenderFn = void (*)(void *canvas, void *map, void *player, uint64_t renderer_id);
// Scheduler task fire: dispatched back to managed code by managed task id.
using DispatchTaskFn = void (*)(uint64_t managed_task_id);

/**
 * Hosts the .NET runtime (hostfxr, resolved via nethost) and exposes the
 * function pointers of the managed bootstrap (Endstone.Loader.dll).
 */
class DotNetHost {
public:
    /**
     * @param runtime_dir directory containing Endstone.Loader.dll,
     *        Endstone.Loader.runtimeconfig.json (and its deps.json)
     */
    explicit DotNetHost(std::filesystem::path runtime_dir);

    /** Starts the runtime and binds the bootstrap exports. Throws std::runtime_error on failure. */
    void start(LogFn log_fn, const void *bridge_table);

    [[nodiscard]] bool isStarted() const { return started_; }

    LoadPluginFn load_plugin = nullptr;
    AttachFn attach = nullptr;
    LifecycleFn on_load = nullptr;
    LifecycleFn on_enable = nullptr;
    LifecycleFn on_disable = nullptr;
    LifecycleFn release = nullptr;
    SetServerFn set_server = nullptr;
    DispatchEventFn dispatch_event = nullptr;
    DispatchCommandFn dispatch_command = nullptr;
    QueryCommandsFn query_commands = nullptr;
    DispatchFormFn dispatch_form = nullptr;
    DispatchMapRenderFn dispatch_map_render = nullptr;
    DispatchTaskFn dispatch_task = nullptr;

private:
    std::filesystem::path runtime_dir_;
    bool started_ = false;
};

}  // namespace dotnet_loader
