using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Domain;

public enum InstallMarketplacePluginStatus
{
    Ok,
    BadRequest,
    NotFound
}

public sealed class InstallMarketplacePluginOutcome
{
    public InstallMarketplacePluginStatus Status { get; init; }
    public PluginInstallationResult? Result { get; init; }
    public object? ErrorBody { get; init; }
}

public interface IInstallMarketplacePluginService
{
    Task<InstallMarketplacePluginOutcome> InstallAsync(string pluginId, Guid userId, CancellationToken cancellationToken = default);
}
