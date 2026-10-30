using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.ValidateMetadata.Repository;

public interface IValidateMetadataRepository
{
    Task<MetadataValidationResult> ValidateEntityMetadataAsync(string entityType, string entityId, Dictionary<string, object> fieldValues);
}
