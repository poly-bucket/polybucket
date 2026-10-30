using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

public enum MetadataRequestStatus { Ok, BadRequest }
public sealed class MetadataSimpleOutcome<T> { public MetadataRequestStatus Status { get; init; } public T? Value { get; init; } public object? ErrorBody { get; init; } }

public interface IGetMetadataEntityFieldsService
{
    Task<MetadataSimpleOutcome<List<MetadataField>>> GetFieldsAsync(string entityType);
}
