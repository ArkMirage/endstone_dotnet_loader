#include "dotnet_plugin_loader.h"

#include <cstdio>
#include <sstream>
#include <unordered_map>

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

std::vector<std::string> split(const std::string &s, char sep)
{
    std::vector<std::string> out;
    std::stringstream ss(s);
    std::string item;
    while (std::getline(ss, item, sep)) {
        if (!item.empty()) {
            out.push_back(item);
        }
    }
    return out;
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
    std::vector<endstone::Command> commands;
    const auto cmd_lines = split(buffer, '\n');
    for (const auto &line : cmd_lines) {
        const auto parts = split(line, '|');
        if (parts.empty()) {
            continue;
        }
        auto usages = parts.size() > 2 ? split(parts[2], ';') : std::vector<std::string>{};
        auto aliases = parts.size() > 3 ? split(parts[3], ';') : std::vector<std::string>{};
        auto permissions = parts.size() > 4 ? split(parts[4], ';') : std::vector<std::string>{};
        commands.emplace_back(parts[0], parts.size() > 1 ? parts[1] : "", std::move(usages), std::move(aliases),
                              std::move(permissions));
    }
    description_ = endstone::PluginDescription(
        description_.getName(), description_.getVersion(), description_.getDescription(), description_.getLoad(),
        description_.getAuthors(), /*contributors=*/{}, /*website=*/"", description_.getPrefix(),
        /*provides=*/{}, /*depend=*/{}, /*soft_depend=*/{}, /*load_before=*/{},
        endstone::PermissionDefault::Operator, std::move(commands));
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

    // Managed side writes "name\nversion\ndescription\nauthor1;author2"
    char info[4096] = {};
    void *gc_handle = host_.load_plugin(file.c_str(), info, sizeof(info));
    if (!gc_handle) {
        logger_.error("Failed to load .NET plugin from '{}': {}", file, info[0] ? info : "unknown error");
        return nullptr;
    }

    const auto lines = split(info, '\n');
    if (lines.size() < 2) {
        logger_.error("Managed plugin '{}' returned malformed metadata.", file);
        host_.release(gc_handle);
        return nullptr;
    }

    // lines[4..] (optional) are command definitions:
    //   "name|description|usage1;usage2|alias1;alias2|permission1;permission2"
    std::vector<endstone::Command> commands;
    for (size_t i = 4; i < lines.size(); ++i) {
        const auto parts = split(lines[i], '|');
        if (parts.empty()) {
            continue;
        }
        auto usages = parts.size() > 2 ? split(parts[2], ';') : std::vector<std::string>{};
        auto aliases = parts.size() > 3 ? split(parts[3], ';') : std::vector<std::string>{};
        auto permissions = parts.size() > 4 ? split(parts[4], ';') : std::vector<std::string>{};
        commands.emplace_back(parts[0], parts.size() > 1 ? parts[1] : "", std::move(usages), std::move(aliases),
                              std::move(permissions));
    }

    endstone::PluginDescription description(
        lines[0], lines[1],
        /*description=*/lines.size() > 2 ? lines[2] : "",
        /*load=*/endstone::PluginLoadOrder::PostWorld,
        /*authors=*/lines.size() > 3 ? split(lines[3], ';') : std::vector<std::string>{},
        /*contributors=*/{}, /*website=*/"", /*prefix=*/"", /*provides=*/{}, /*depend=*/{}, /*soft_depend=*/{},
        /*load_before=*/{}, /*default_permission=*/endstone::PermissionDefault::Operator,
        /*commands=*/std::move(commands));

    auto plugin = std::make_unique<DotNetPlugin>(host_, gc_handle, std::move(description));
    host_.attach(gc_handle, plugin.get());
    host_.set_server(&getServer());
    pluginLookup()[gc_handle] = plugin.get();
    auto *ptr = plugin.get();
    plugins_.push_back(std::move(plugin));  // loader keeps ownership, same as the Python loader
    return ptr;
}

}  // namespace dotnet_loader
