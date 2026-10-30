namespace PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

public class GetPluginsItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required string Description { get; init; }
}
