using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.ValidateMetadata.Http;

namespace PolyBucket.Api.Features.Plugins.ValidateMetadata.Domain;

public interface IValidateMetadataService
{
    Task<MetadataSimpleOutcome<MetadataValidationResult>> ValidateAsync(string entityType, string entityId, MetadataValidationRequest request);
}
