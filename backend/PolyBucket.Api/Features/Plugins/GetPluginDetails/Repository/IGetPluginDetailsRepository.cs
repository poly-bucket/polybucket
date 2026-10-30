using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Repository;

public interface IGetPluginDetailsRepository
{
    IPlugin? FindPlugin(string pluginId);
}
