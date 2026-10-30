using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.ReloadPlugins.Repository;

public class ReloadPluginsRepository(PluginManager pluginManager) : IReloadPluginsRepository
{
    private readonly PluginManager _pluginManager = pluginManager;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _pluginManager.UnloadAllPluginsAsync();
        await _pluginManager.InitializeAsync();
    }
}
