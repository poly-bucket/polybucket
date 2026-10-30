using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataSchema.Repository;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Domain;

public class GetMetadataSchemaService(IGetMetadataSchemaRepository repository) : IGetMetadataSchemaService
{
    public async Task<MetadataSimpleOutcome<MetadataSchema>> GetSchemaAsync(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new() { Status = MetadataRequestStatus.BadRequest, ErrorBody = new { message = "Entity type is required" } };
        var fields = await repository.GetEntityFieldsAsync(entityType);
        var defaultValues = await repository.GetDefaultEntityMetadataAsync(entityType);
        return new()
        {
            Status = MetadataRequestStatus.Ok,
            Value = new MetadataSchema
            {
                EntityType = entityType,
                Fields = fields,
                DefaultValues = defaultValues,
                GeneratedAt = DateTime.UtcNow
            }
        };
    }
}
