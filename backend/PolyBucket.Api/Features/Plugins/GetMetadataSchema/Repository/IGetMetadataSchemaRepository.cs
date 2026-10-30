using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Repository;

public interface IGetMetadataSchemaRepository
{
    Task<List<MetadataField>> GetEntityFieldsAsync(string entityType);
    Task<Dictionary<string, object>> GetDefaultEntityMetadataAsync(string entityType);
}
