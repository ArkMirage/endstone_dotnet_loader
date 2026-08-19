using System.Collections.Generic;

namespace Endstone.Loader;

/// <summary>
/// Base class for a custom plugin loader that a .NET plugin can register with
/// <see cref="PluginManager.RegisterLoader"/>. The loader scans a directory for
/// files matching <see cref="FileFilters"/> and turns each match into a
/// <see cref="PluginBase"/> via <see cref="LoadPlugin"/>.
///
/// A loaded plugin is driven exactly like a normal .NET plugin: its OnLoad /
/// OnEnable / OnDisable, event registrations, commands and permissions all go
/// through the same machinery, so the only thing a loader author implements is
/// "given a file, build a PluginBase instance". The native proxy, GCHandle
/// ownership, dependency resolution and lifecycle wiring are handled by the
/// loader framework.
///
/// <example>
/// <code>
/// public sealed class LuaLoader : PluginLoader
/// {
///     public override IReadOnlyList&lt;string&gt; FileFilters =&gt; new[] { "\\.lua\\.dll$" };
///     public override PluginBase? LoadPlugin(string filePath)
///     {
///         var code = File.ReadAllText(filePath);
///         return new LuaPlugin(code);
///     }
/// }
/// </code>
/// </example>
/// </summary>
public abstract class PluginLoader
{
    /// <summary>
    /// Filename filters (ECMAScript regular expressions, the same syntax
    /// endstone::PluginLoader uses) that identify the files this loader handles,
    /// e.g. <c>"\\.lua\\.dll$"</c>. A file is offered to the loader when any
    /// filter matches its path.
    /// </summary>
    public abstract IReadOnlyList<string> FileFilters { get; }

    /// <summary>
    /// Instantiates and returns the plugin described by <paramref name="filePath"/>,
    /// or <c>null</c> to skip the file. Set the returned instance's
    /// <see cref="Plugin.Description"/> (typically via a <see cref="PluginAttribute"/>
    /// on its class) before returning; an empty name is rejected by the server.
    /// </summary>
    public abstract PluginBase? LoadPlugin(string filePath);
}
