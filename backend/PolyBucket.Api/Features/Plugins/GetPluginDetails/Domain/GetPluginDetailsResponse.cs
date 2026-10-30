using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Domain;

public sealed class GetPluginDetailsResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsLoaded { get; init; }
    public PluginMetadata? Metadata { get; init; }
    public List<PluginComponent>? Components { get; init; }
    public DateTime LoadedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<string> Dependencies { get; init; } = new();
    public List<PluginHook> Hooks { get; init; } = new();
    public List<PluginSetting> Settings { get; init; } = new();
}
