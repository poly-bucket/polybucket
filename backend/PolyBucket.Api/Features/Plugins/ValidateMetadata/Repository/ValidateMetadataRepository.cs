using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.ValidateMetadata.Repository;

public class ValidateMetadataRepository(MetadataPluginService metadataPluginService) : IValidateMetadataRepository
{
    public Task<MetadataValidationResult> ValidateEntityMetadataAsync(string entityType, string entityId, Dictionary<string, object> fieldValues) =>
        metadataPluginService.ValidateEntityMetadataAsync(entityType, entityId, fieldValues);
}
