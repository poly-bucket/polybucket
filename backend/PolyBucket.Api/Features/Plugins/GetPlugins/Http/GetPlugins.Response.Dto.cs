namespace PolyBucket.Api.Features.Plugins.GetPlugins.Http;

public class GetPluginsResponseDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required string Description { get; init; }
}
