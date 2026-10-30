namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Domain;

public sealed class GetPluginHealthResponse
{
    public int TotalPlugins { get; init; }
    public int ActivePlugins { get; init; }
    public int InactivePlugins { get; init; }
    public int ErrorPlugins { get; init; }
    public IReadOnlyList<GetPluginHealthPluginSummary> Plugins { get; init; } = Array.Empty<GetPluginHealthPluginSummary>();
}

public sealed class GetPluginHealthPluginSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
