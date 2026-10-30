namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Domain;

public interface IGetPluginHealthService
{
    Task<GetPluginHealthResponse> GetHealthAsync(CancellationToken cancellationToken = default);
}
