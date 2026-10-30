using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Repository;

public interface IUpdatePluginStatusRepository
{
    IPlugin? FindPlugin(string pluginId);
}
