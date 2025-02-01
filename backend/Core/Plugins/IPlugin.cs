using System.ComponentModel.Composition;

namespace Core.Plugins
{
    /// <summary>
    /// Base interface that all plugins must implement
    /// </summary>
    [InheritedExport]
    public interface IPlugin
    {
        /// <summary>
        /// Unique identifier for the plugin
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the plugin
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Author of the plugin
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Description of what the plugin does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Initialize the plugin
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Cleanup when plugin is being unloaded
        /// </summary>
        Task UnloadAsync();
    }
} 