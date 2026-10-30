using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

public interface IGetPluginsService
{
    IReadOnlyList<GetPluginsItem> GetPlugins();
}
