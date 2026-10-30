namespace PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Repository;

public interface IGetDefaultMetadataRepository
{
    Task<Dictionary<string, object>> GetDefaultEntityMetadataAsync(string entityType);
}
