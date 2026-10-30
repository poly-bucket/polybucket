using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.InstallPlugin.Domain;

public sealed class InstallPluginOutcome
{
    public InstallPluginHttpStatus HttpStatus { get; init; }
    public PluginInstallationResult? Result { get; init; }
    public object? ErrorBody { get; init; }
}

public enum InstallPluginHttpStatus
{
    Ok,
    BadRequest,
    NotFound
}

public interface IInstallPluginService
{
    Task<InstallPluginOutcome> InstallAsync(PluginInstallationRequest request, CancellationToken cancellationToken = default);
}
