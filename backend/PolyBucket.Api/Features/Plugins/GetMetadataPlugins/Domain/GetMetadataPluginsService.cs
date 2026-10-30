using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Repository;

namespace PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Domain;

public class GetMetadataPluginsService(IGetMetadataPluginsRepository repository) : IGetMetadataPluginsService
{
    public List<MetadataPluginInfo> GetPlugins() => repository.GetRegisteredMetadataPlugins();
}
