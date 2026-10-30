using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Repository;

public class GetDefaultMetadataRepository(MetadataPluginService metadataPluginService) : IGetDefaultMetadataRepository
{
    public Task<Dictionary<string, object>> GetDefaultEntityMetadataAsync(string entityType) =>
        metadataPluginService.GetDefaultEntityMetadataAsync(entityType);
}
