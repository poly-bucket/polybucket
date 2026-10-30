using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginsOverview.Repository;

public interface IGetPluginsOverviewRepository
{
    IReadOnlyList<IPlugin> GetLoadedPlugins();
}
