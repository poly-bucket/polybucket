using PolyBucket.Api.Features.Plugins.GetPluginDetails.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Domain;

public class GetPluginDetailsService(IGetPluginDetailsRepository repository) : IGetPluginDetailsService
{
    public GetPluginDetailsResponse? GetDetails(string pluginId)
    {
        var plugin = repository.FindPlugin(pluginId);
        if (plugin == null)
        {
            return null;
        }

        return new GetPluginDetailsResponse
        {
            Id = plugin.Id,
            Name = plugin.Name,
            Version = plugin.Version,
            Author = plugin.Author,
            Description = plugin.Description,
            IsLoaded = true,
            Metadata = plugin.Metadata,
            Components = plugin.FrontendComponents?.ToList() ?? new(),
            LoadedAt = DateTime.UtcNow,
            Status = "Active",
            Dependencies = plugin.Metadata?.Dependencies ?? new List<string>(),
            Hooks = plugin.FrontendComponents?.SelectMany(c => c.Hooks).ToList() ?? new(),
            Settings = plugin.Metadata?.Settings?.Values.ToList() ?? new()
        };
    }
}
