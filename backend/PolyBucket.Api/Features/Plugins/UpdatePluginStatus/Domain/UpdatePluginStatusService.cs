using PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Http;
using PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Repository;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Domain;

public class UpdatePluginStatusService(IUpdatePluginStatusRepository repository) : IUpdatePluginStatusService
{
    public PluginStatusResponse? UpdateStatus(string pluginId, UpdatePluginStatusRequest request)
    {
        var plugin = repository.FindPlugin(pluginId);
        if (plugin == null)
        {
            return null;
        }

        if (!request.Enabled && plugin.Metadata?.Lifecycle?.CanDisable == false)
        {
            return new PluginStatusResponse
            {
                Success = false,
                Message = "This plugin cannot be disabled",
                CurrentStatus = "Active"
            };
        }

        var newStatus = request.Enabled ? "Active" : "Disabled";
        return new PluginStatusResponse
        {
            Success = true,
            Message = $"Plugin {(request.Enabled ? "enabled" : "disabled")} successfully",
            CurrentStatus = newStatus,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
