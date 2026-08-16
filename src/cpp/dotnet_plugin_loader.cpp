#include "dotnet_plugin_loader.h"

#include <algorithm>
#include <cstdio>
#include <optional>
#include <unordered_map>

#include <nlohmann/json.hpp>

#include "bridge.h"

namespace dotnet_loader {

namespace {

// gc_handle -> DotNetPlugin* so the event-registration bridge can resolve
// the native proxy owning a managed plugin instance.
std::unordered_map<void *, DotNetPlugin *> &pluginLookup()
{
    static std::unordered_map<void *, DotNetPlugin *> lookup;
    return lookup;
}

endstone::Logger *s_event_logger = nullptr;
DotNetHost *s_host = nullptr;

}  // namespace

DotNetPluginLoader::DotNetPluginLoader(endstone::Server &server, DotNetHost &host, endstone::Logger &logger)
    : PluginLoader(server), host_(host), logger_(logger)
{
    s_event_logger = &logger;
    s_host = &host;
}

// Converts a JSON command object into an endstone::Command.
endstone::Command parseCommand(const nlohmann::json &obj)
{
    return endstone::Command(obj.value("name", std::string{}), obj.value("description", std::string{}),
                             obj.value("usages", std::vector<std::string>{}),
                             obj.value("aliases", std::vector<std::string>{}),
                             obj.value("permissions", std::vector<std::string>{}));
}

// Parses a JSON array of command objects.
std::vector<endstone::Command> parseCommands(const nlohmann::json &arr)
{
    std::vector<endstone::Command> commands;
    if (arr.is_array()) {
        for (const auto &item : arr) {
            commands.push_back(parseCommand(item));
        }
    }
    return commands;
}

// Parses the numeric default-permission value sent by the managed side. The
// value must match the C# PermissionDefault enum order, which mirrors the C++
// endstone::PermissionDefault declaration order (True=0 ... Console=4).
endstone::PermissionDefault parsePermissionDefault(const nlohmann::json &doc)
{
    const auto value =
        doc.value("defaultPermission", static_cast<int>(endstone::PermissionDefault::Operator));
    return static_cast<endstone::PermissionDefault>(std::clamp(value, 0, 4));
}

// Parses the JSON plugin info object into an endstone::PluginDescription.
// The managed side serializes plugin info with camelCase keys (JsonNamingPolicy.CamelCase),
// e.g. {"name","version","description","authors","contributors","website","prefix",
// "depend","softDepend","loadBefore","defaultPermission","commands"}.
// Returns nullopt on malformed input.
std::optional<endstone::PluginDescription> parseDescription(const char *info)
{
    const auto doc = nlohmann::json::parse(info, nullptr, false);
    if (doc.is_discarded()) {
        return std::nullopt;
    }
    return endstone::PluginDescription(
        doc.value("name", std::string{}), doc.value("version", std::string{}),
        doc.value("description", std::string{}),
        /*load=*/endstone::PluginLoadOrder::PostWorld,
        /*authors=*/doc.value("authors", std::vector<std::string>{}),
        /*contributors=*/doc.value("contributors", std::vector<std::string>{}),
        /*website=*/doc.value("website", std::string{}), /*prefix=*/doc.value("prefix", std::string{}),
        /*provides=*/{}, /*depend=*/doc.value("depend", std::vector<std::string>{}),
        /*soft_depend=*/doc.value("softDepend", std::vector<std::string>{}),
        /*load_before=*/doc.value("loadBefore", std::vector<std::string>{}),
        /*default_permission=*/parsePermissionDefault(doc),
        /*commands=*/parseCommands(doc.value("commands", nlohmann::json::array())));
}

DotNetPlugin::~DotNetPlugin()
{
    if (gc_handle_) {
        host_.release(gc_handle_);
        pluginLookup().erase(gc_handle_);
    }
}

void DotNetPlugin::addEventListener(std::string event_name, int priority, bool ignore_cancelled, void *cb_handle)
{
    event_registrations_.push_back({std::move(event_name), priority, ignore_cancelled, cb_handle});
}

void DotNetPlugin::onLoad()
{
    host_.on_load(gc_handle_);

    // The managed OnLoad may declare commands; the command map is populated
    // between load and enable, so re-query the declared commands now.
    refreshCommands();
}

void DotNetPlugin::refreshCommands()
{
    if (!host_.query_commands) {
        return;
    }
    char buffer[16384] = {};
    if (host_.query_commands(gc_handle_, buffer, sizeof(buffer)) <= 0) {
        return;
    }
    // The managed side writes a JSON array of command objects
    // [{"name","description","usages","aliases","permissions"}, ...].
    const auto doc = nlohmann::json::parse(buffer, nullptr, false);
    if (doc.is_discarded()) {
        // Keep the previously loaded description (commands registered in OnLoad).
        getServer().getLogger().error("Failed to parse command JSON from plugin '{}'.", description_.getName());
        return;
    }
    const auto commands = parseCommands(doc);
    // Rebuild the description with only the commands replaced; every other
    // field is carried over from the previously parsed plugin info.
    description_ = endstone::PluginDescription(
        description_.getName(), description_.getVersion(), description_.getDescription(), description_.getLoad(),
        description_.getAuthors(), description_.getContributors(), description_.getWebsite(),
        description_.getPrefix(), description_.getProvides(), description_.getDepend(),
        description_.getSoftDepend(), description_.getLoadBefore(), description_.getDefaultPermission(), commands,
        description_.getPermissions());
}

void DotNetPlugin::flushEventListeners()
{
    if (event_registrations_.empty()) {
        return;
    }
    auto &pm = getServer().getPluginManager();
    for (const auto &reg : event_registrations_) {
        auto priority = static_cast<endstone::EventPriority>(std::clamp(reg.priority, 0, 5));
        pm.registerEvent(
            reg.event_name,
            [this, cb = reg.cb_handle](endstone::Event &e) { host_.dispatch_event(gc_handle_, cb, &e); }, priority,
            *this, reg.ignore_cancelled);
    }
}

bool DotNetPlugin::onCommand(endstone::CommandSender &sender, const endstone::Command &command,
                             const std::vector<std::string> &args)
{
    if (!host_.dispatch_command) {
        return false;
    }
    std::vector<const char *> arg_ptrs;
    arg_ptrs.reserve(args.size());
    for (const auto &arg : args) {
        arg_ptrs.push_back(arg.c_str());
    }
    return host_.dispatch_command(gc_handle_, &sender, command.getName().c_str(), arg_ptrs.data(),
                                  static_cast<int32_t>(arg_ptrs.size())) != 0;
}

void bridgeRegisterEvent(void *gc_handle, const char *event_name, int priority, bool ignore_cancelled, void *cb_handle)
{
    auto &lookup = pluginLookup();
    const auto it = lookup.find(gc_handle);
    if (it == lookup.end()) {
        if (s_event_logger) {
            char buf[32];
            std::snprintf(buf, sizeof(buf), "%p", gc_handle);
            s_event_logger->warning("[DotNetLoader] Event registration dropped: unknown plugin handle {}. "
                                    "Managed side must pass the gc_handle.",
                                    buf);
        }
        return;
    }
    it->second->addEventListener(event_name ? event_name : "", priority, ignore_cancelled, cb_handle);
}

void bridgeFormDispatchResult(void *player, int result_kind, uint64_t form_id, int button_index, const char *payload)
{
    if (s_host && s_host->dispatch_form) {
        s_host->dispatch_form(player, result_kind, form_id, button_index, payload);
    }
}

void bridgeMapRenderCallback(void *canvas, void *map, void *player, uint64_t renderer_id)
{
    if (s_host && s_host->dispatch_map_render) {
        s_host->dispatch_map_render(canvas, map, player, renderer_id);
    }
}

void bridgeSchedulerTaskCallback(uint64_t managed_task_id)
{
    if (s_host && s_host->dispatch_task) {
        s_host->dispatch_task(managed_task_id);
    }
}

void installEventBridge()
{
    mutableBridgeTable().plugin_register_event = &bridgeRegisterEvent;
    mutableBridgeTable().form_dispatch_result = &bridgeFormDispatchResult;
    mutableBridgeTable().map_render_callback = &bridgeMapRenderCallback;
    mutableBridgeTable().scheduler_task_callback = &bridgeSchedulerTaskCallback;
}

std::vector<std::string> DotNetPluginLoader::getPluginFileFilters() const
{
    // Only match *.Plugin.dll to avoid clashing with the C++ loader's "\.dll$"
    return {"\\.Plugin\\.dll$"};
}

endstone::Plugin *DotNetPluginLoader::loadPlugin(std::string file)
{
    if (!host_.isStarted()) {
        logger_.error(".NET runtime is not running; cannot load '{}'", file);
        return nullptr;
    }

    // Managed side writes a JSON object:
    // {"name","version","description","authors","commands":[...]}
    char info[4096] = {};
    void *gc_handle = host_.load_plugin(file.c_str(), info, sizeof(info));
    if (!gc_handle) {
        logger_.error("Failed to load .NET plugin from '{}': {}", file, info[0] ? info : "unknown error");
        return nullptr;
    }

    auto description = parseDescription(info);
    if (!description) {
        logger_.error("Managed plugin '{}' returned malformed metadata.", file);
        host_.release(gc_handle);
        return nullptr;
    }

    auto plugin = std::make_unique<DotNetPlugin>(host_, gc_handle, std::move(*description));
    host_.attach(gc_handle, plugin.get());
    host_.set_server(&getServer());
    pluginLookup()[gc_handle] = plugin.get();
    auto *ptr = plugin.get();
    plugins_.push_back(std::move(plugin));  // loader keeps ownership, same as the Python loader
    return ptr;
}

}  // namespace dotnet_loader
