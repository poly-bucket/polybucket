namespace PolyBucket.Api.Features.Plugins.UninstallPlugin.Domain;

public enum UninstallPluginStatus
{
    Ok,
    BadRequest,
    NotFound
}

public sealed class UninstallPluginOutcome
{
    public UninstallPluginStatus Status { get; init; }
    public string PluginId { get; init; } = string.Empty;
}

public interface IUninstallPluginService
{
    UninstallPluginOutcome Uninstall(string pluginId);
    Task<UninstallPluginOutcome> UninstallAsync(string pluginId, CancellationToken cancellationToken = default);
}
