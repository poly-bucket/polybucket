using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginHooks.Repository;

public interface IGetPluginHooksRepository
{
    IReadOnlyList<IPlugin> GetLoadedPlugins();
}
