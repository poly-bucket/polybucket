using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.TransformMetadata.Http;

namespace PolyBucket.Api.Features.Plugins.TransformMetadata.Domain;

public interface ITransformMetadataService
{
    Task<MetadataSimpleOutcome<Dictionary<string, object>>> TransformAsync(string entityType, MetadataTransformRequest request);
}
