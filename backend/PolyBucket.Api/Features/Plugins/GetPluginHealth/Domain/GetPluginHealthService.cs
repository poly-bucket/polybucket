using PolyBucket.Api.Features.Plugins.GetPluginHealth.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Domain;

public class GetPluginHealthService(IGetPluginHealthRepository repository) : IGetPluginHealthService
{
    public async Task<GetPluginHealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        var installedPlugins = await repository.GetInstalledPluginsAsync(cancellationToken);
        return new GetPluginHealthResponse
        {
            TotalPlugins = installedPlugins.Count,
            ActivePlugins = installedPlugins.Count(p => p.Status == "active"),
            InactivePlugins = installedPlugins.Count(p => p.Status == "inactive"),
            ErrorPlugins = installedPlugins.Count(p => p.Status == "error"),
            Plugins = installedPlugins.Select(p => new GetPluginHealthPluginSummary
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                Status = p.Status,
                IsEnabled = p.IsEnabled
            }).ToList()
        };
    }
}
