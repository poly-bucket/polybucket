using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.TransformMetadata.Http;
using PolyBucket.Api.Features.Plugins.TransformMetadata.Repository;

namespace PolyBucket.Api.Features.Plugins.TransformMetadata.Domain;

public class TransformMetadataService(ITransformMetadataRepository repository) : ITransformMetadataService
{
    public async Task<MetadataSimpleOutcome<Dictionary<string, object>>> TransformAsync(string entityType, MetadataTransformRequest request)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity type is required" } };
        if (request.FieldValues == null)
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Field values are required" } };
        return new() { Status = MetadataRequestStatus.Ok, Value = await repository.TransformEntityMetadataAsync(entityType, request.FieldValues) };
    }
}
