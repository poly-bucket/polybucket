namespace PolyBucket.Api.Features.Plugins.GetPluginsOverview.Domain;

public sealed class GetPluginsOverviewResponse
{
    public int TotalPlugins { get; init; }
    public int ActivePlugins { get; init; }
    public IReadOnlyList<PluginSummary> PluginSummaries { get; init; } = Array.Empty<PluginSummary>();
    public DateTime LastRefresh { get; init; }
}

public sealed class PluginSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsLoaded { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ComponentCount { get; init; }
    public int HookCount { get; init; }
    public int SettingsCount { get; init; }
    public bool HasPermissions { get; init; }
}
