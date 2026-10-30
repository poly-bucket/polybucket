using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Repository;

public class GetMetadataSchemaRepository(MetadataPluginService metadataPluginService) : IGetMetadataSchemaRepository
{
    public Task<List<MetadataField>> GetEntityFieldsAsync(string entityType) => metadataPluginService.GetEntityFieldsAsync(entityType);
    public Task<Dictionary<string, object>> GetDefaultEntityMetadataAsync(string entityType) => metadataPluginService.GetDefaultEntityMetadataAsync(entityType);
}
