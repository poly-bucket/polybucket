using PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Http;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Domain;

public interface IUpdatePluginStatusService
{
    PluginStatusResponse? UpdateStatus(string pluginId, UpdatePluginStatusRequest request);
}
