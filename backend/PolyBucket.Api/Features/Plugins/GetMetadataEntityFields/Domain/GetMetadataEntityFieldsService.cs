using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Repository;

namespace PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

public class GetMetadataEntityFieldsService(IGetMetadataEntityFieldsRepository repository) : IGetMetadataEntityFieldsService
{
    public async Task<MetadataSimpleOutcome<List<MetadataField>>> GetFieldsAsync(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity type is required" } };
        return new() { Status = MetadataRequestStatus.Ok, Value = await repository.GetEntityFieldsAsync(entityType) };
    }
}
