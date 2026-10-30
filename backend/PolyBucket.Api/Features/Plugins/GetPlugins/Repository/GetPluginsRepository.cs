using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Repository;

public class GetPluginsRepository(PluginManager pluginManager) : IGetPluginsRepository
{
    private readonly PluginManager _pluginManager = pluginManager;

    public IReadOnlyList<GetPluginsItem> GetLoadedPlugins()
    {
        return _pluginManager.GetLoadedPlugins()
            .Select(p => new GetPluginsItem
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                Author = p.Author,
                Description = p.Description
            })
            .ToList();
    }
}
