using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Domain;

public interface IGetMetadataSchemaService
{
    Task<MetadataSimpleOutcome<MetadataSchema>> GetSchemaAsync(string entityType);
}
