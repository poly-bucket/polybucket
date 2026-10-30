namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Domain;

public interface IGetPluginDetailsService
{
    GetPluginDetailsResponse? GetDetails(string pluginId);
}
