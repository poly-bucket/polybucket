using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Domain;

public sealed class MetadataSchema
{
    public string EntityType { get; init; } = string.Empty;
    public List<MetadataField> Fields { get; init; } = new();
    public Dictionary<string, object> DefaultValues { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}
