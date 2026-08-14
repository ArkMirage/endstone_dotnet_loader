#include <filesystem>
#include <memory>

#include <endstone/endstone.hpp>

#include "bridge.h"
#include "dotnet_host.h"
#include "dotnet_plugin_loader.h"
#include "version.h"

namespace fs = std::filesystem;

class DotNetLoaderPlugin;
namespace {
DotNetLoaderPlugin *g_plugin = nullptr;
}

class DotNetLoaderPlugin : public endstone::Plugin {
public:
    void onLoad() override
    {
        g_plugin = this;

        // Runtime files live in plugins/dotnet_loader/runtime/
        const auto runtime_dir = getDataFolder() / "runtime";
        dotnet_loader::installEventBridge();
        host_ = std::make_unique<dotnet_loader::DotNetHost>(runtime_dir);

        try {
            host_->start(&DotNetLoaderPlugin::logCallback, &dotnet_loader::getBridgeTable());
        }
        catch (const std::exception &e) {
            getLogger().error("Failed to start the .NET runtime: {}", e.what());
            getLogger().error("Place Endstone.Loader.dll and its runtimeconfig.json in '{}'.",
                              runtime_dir.string());
            return;
        }
        getLogger().info(".NET runtime started.");

        auto &pm = getServer().getPluginManager();
        auto loader = std::make_unique<dotnet_loader::DotNetPluginLoader>(getServer(), *host_, getLogger());
        loader_ = loader.get();
        pm.registerLoader(std::move(loader));

        // Loaders registered during onLoad are excluded from the ongoing scan
        // (the plugin manager iterates a snapshot), so trigger our own pass.
        // .NET plugin assemblies live in <server>/plugins.net/
        const auto plugins_net = fs::current_path() / "plugins.net";
        std::error_code ec;
        fs::create_directories(plugins_net, ec);
        const auto loaded = pm.loadPlugins(plugins_net.string());
        getLogger().info("Loaded {} .NET plugin(s) from '{}'.", loaded.size(), plugins_net.string());
    }

    void onDisable() override { g_plugin = nullptr; }

private:
    static void logCallback(void *plugin, int level, const char *message)
    {
        if (!message) {
            return;
        }
        auto lvl = static_cast<endstone::Logger::Level>(std::clamp(level, 0, 5));
        if (auto *p = static_cast<endstone::Plugin *>(plugin)) {
            p->getLogger().log(lvl, message);
        }
        else if (g_plugin) {
            g_plugin->getLogger().log(lvl, message);
        }
    }

    std::unique_ptr<dotnet_loader::DotNetHost> host_;
    dotnet_loader::DotNetPluginLoader *loader_ = nullptr;
};

ENDSTONE_PLUGIN("dotnet_loader", DOTNET_LOADER_VERSION, DotNetLoaderPlugin)
{
    prefix = "DotNetLoader";
    description = ".NET (CLR) plugin loader for Endstone";
    authors = {"dotnet-endstone"};
    load = endstone::PluginLoadOrder::Startup;
}
