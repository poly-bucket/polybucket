namespace PolyBucket.Api.Features.Plugins.TransformMetadata.Repository;

public interface ITransformMetadataRepository
{
    Task<Dictionary<string, object>> TransformEntityMetadataAsync(string entityType, Dictionary<string, object> fieldValues);
}
