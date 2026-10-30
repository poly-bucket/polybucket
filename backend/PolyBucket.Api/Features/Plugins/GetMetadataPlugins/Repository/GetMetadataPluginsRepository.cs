using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Repository;

public class GetMetadataPluginsRepository(MetadataPluginService metadataPluginService) : IGetMetadataPluginsRepository
{
    public List<MetadataPluginInfo> GetRegisteredMetadataPlugins() => metadataPluginService.GetRegisteredMetadataPlugins();
}
