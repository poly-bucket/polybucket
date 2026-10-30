using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.ValidateMetadata.Http;
using PolyBucket.Api.Features.Plugins.ValidateMetadata.Repository;

namespace PolyBucket.Api.Features.Plugins.ValidateMetadata.Domain;

public class ValidateMetadataService(IValidateMetadataRepository repository) : IValidateMetadataService
{
    public async Task<MetadataSimpleOutcome<MetadataValidationResult>> ValidateAsync(string entityType, string entityId, MetadataValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity type is required" } };
        if (string.IsNullOrWhiteSpace(entityId))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity ID is required" } };
        if (request.FieldValues == null)
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Field values are required" } };
        var result = await repository.ValidateEntityMetadataAsync(entityType, entityId, request.FieldValues);
        return new() { Status = MetadataRequestStatus.Ok, Value = result };
    }
}
