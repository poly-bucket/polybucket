using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Repository;

public interface IGetMetadataEntityFieldsRepository
{
    Task<List<MetadataField>> GetEntityFieldsAsync(string entityType);
}
