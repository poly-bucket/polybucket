using PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Repository;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

namespace PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Domain;

public class GetDefaultMetadataService(IGetDefaultMetadataRepository repository) : IGetDefaultMetadataService
{
    public async Task<MetadataSimpleOutcome<Dictionary<string, object>>> GetDefaultsAsync(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity type is required" } };
        return new() { Status = MetadataRequestStatus.Ok, Value = await repository.GetDefaultEntityMetadataAsync(entityType) };
    }
}
