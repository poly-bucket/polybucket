using System.ComponentModel.Composition;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Examples.Plugins.SampleDual;

[Export(typeof(IPlugin))]
public class SampleDualPlugin : IPlugin
{
    public string Id => "sample-dual-plugin";
    public string Name => "Sample Dual Plugin";
    public string Version => "1.0.0";
    public string Author => "PolyBucket Community";
    public string Description => "API and frontend assets for marketplace install tests";

    public IEnumerable<PluginComponent> FrontendComponents => Array.Empty<PluginComponent>();

    public PluginMetadata Metadata { get; } = new()
    {
        MinimumAppVersion = "1.0.0",
        MaximumAppVersion = "2.0.0"
    };

    public Task InitializeAsync() => Task.CompletedTask;

    public Task UnloadAsync() => Task.CompletedTask;
}
