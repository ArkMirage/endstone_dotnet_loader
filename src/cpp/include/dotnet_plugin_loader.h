#pragma once

#include <memory>
#include <string>
#include <vector>

#include <endstone/endstone.hpp>

#include "dotnet_host.h"

namespace dotnet_loader {

struct EventRegistration {
    std::string event_name;
    int priority = 2;  // EventPriority::Normal
    bool ignore_cancelled = false;
    void *cb_handle = nullptr;
};

/**
 * Native proxy for a managed plugin instance. Owns the GCHandle and forwards
 * lifecycle calls into managed code via the host's function pointers.
 */
class DotNetPlugin : public endstone::Plugin {
public:
    DotNetPlugin(DotNetHost &host, void *gc_handle, endstone::PluginDescription description)
        : host_(host), gc_handle_(gc_handle), description_(std::move(description))
    {
    }

    ~DotNetPlugin() override;

    [[nodiscard]] const endstone::PluginDescription &getDescription() const override { return description_; }

    void onLoad() override;

    void onEnable() override
    {
        // The managed OnEnable may register event handlers; flush them after
        // so those registrations take effect too.
        host_.on_enable(gc_handle_);
        flushEventListeners();
    }

    void onDisable() override { host_.on_disable(gc_handle_); }

    bool onCommand(endstone::CommandSender &sender, const endstone::Command &command,
                   const std::vector<std::string> &args) override;

    void addEventListener(std::string event_name, int priority, bool ignore_cancelled, void *cb_handle);

private:
    void flushEventListeners();
    void refreshCommands();

    DotNetHost &host_;
    void *gc_handle_;
    endstone::PluginDescription description_;
    std::vector<EventRegistration> event_registrations_;
};

/** Registers the plugin_register_event bridge; called once from the loader plugin's onLoad. */
void installEventBridge();

/**
 * PluginLoader for .NET assemblies. Scans for *.Plugin.dll files inside the
 * plugins directory and loads them through the hosted CLR.
 */
class DotNetPluginLoader : public endstone::PluginLoader {
public:
    DotNetPluginLoader(endstone::Server &server, DotNetHost &host, endstone::Logger &logger);

    [[nodiscard]] endstone::Plugin *loadPlugin(std::string file) override;
    [[nodiscard]] std::vector<std::string> getPluginFileFilters() const override;

private:
    DotNetHost &host_;
    endstone::Logger &logger_;
    std::vector<std::unique_ptr<DotNetPlugin>> plugins_;
};

}  // namespace dotnet_loader
