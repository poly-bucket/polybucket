using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

namespace PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Domain;

public interface IGetDefaultMetadataService
{
    Task<MetadataSimpleOutcome<Dictionary<string, object>>> GetDefaultsAsync(string entityType);
}
