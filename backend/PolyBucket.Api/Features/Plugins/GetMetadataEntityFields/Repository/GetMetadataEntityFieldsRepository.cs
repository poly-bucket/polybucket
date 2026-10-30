using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Repository;

public class GetMetadataEntityFieldsRepository(MetadataPluginService metadataPluginService) : IGetMetadataEntityFieldsRepository
{
    public Task<List<MetadataField>> GetEntityFieldsAsync(string entityType) => metadataPluginService.GetEntityFieldsAsync(entityType);
}
