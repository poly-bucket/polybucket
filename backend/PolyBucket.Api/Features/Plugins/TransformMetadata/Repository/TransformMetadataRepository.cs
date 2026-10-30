using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.TransformMetadata.Repository;

public class TransformMetadataRepository(MetadataPluginService metadataPluginService) : ITransformMetadataRepository
{
    public Task<Dictionary<string, object>> TransformEntityMetadataAsync(string entityType, Dictionary<string, object> fieldValues) =>
        metadataPluginService.TransformEntityMetadataAsync(entityType, fieldValues);
}
