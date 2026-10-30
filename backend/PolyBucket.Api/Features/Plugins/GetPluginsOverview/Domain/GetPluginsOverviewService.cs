using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPluginsOverview.Domain;

public class GetPluginsOverviewService(IGetPluginsOverviewRepository repository) : IGetPluginsOverviewService
{
    public GetPluginsOverviewResponse GetOverview()
    {
        var loadedPlugins = repository.GetLoadedPlugins();
        var plugins = loadedPlugins.Select(p => new PluginSummary
        {
            Id = p.Id,
            Name = p.Name,
            Version = p.Version,
            Author = p.Author,
            Description = p.Description,
            IsLoaded = true,
            Status = "Active",
            ComponentCount = p.FrontendComponents?.Count() ?? 0,
            HookCount = p.FrontendComponents?.SelectMany(c => c.Hooks).Count() ?? 0,
            SettingsCount = p.Metadata?.Settings?.Count ?? 0,
            HasPermissions = p.Metadata?.RequiredPermissions?.Any() ?? false
        }).ToList();

        return new GetPluginsOverviewResponse
        {
            TotalPlugins = plugins.Count,
            ActivePlugins = plugins.Count(p => p.Status == "Active"),
            PluginSummaries = plugins,
            LastRefresh = DateTime.UtcNow
        };
    }
}
