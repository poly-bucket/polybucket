using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services
{
    public interface IPluginService
    {
        Task<List<Plugin>> GetPluginsAsync(string? category = null, string? search = null);
        Task<PluginBrowseResponse> BrowsePluginsAsync(PluginBrowseRequest request);
        Task<Plugin?> GetPluginAsync(string id);
        Task<Plugin> CreatePluginAsync(Plugin plugin);
        Task<Plugin> UpdatePluginAsync(Plugin plugin);
        Task<bool> DeletePluginAsync(string id);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetPopularTagsAsync(int limit = 20);
        Task<List<PluginSummary>> GetFeaturedPluginsAsync(int limit = 6);
        Task<List<PluginSummary>> GetTrendingPluginsAsync(int limit = 10);
        
        // Installation-related methods
        Task<PluginDownloadInfo?> GetPluginDownloadAsync(string pluginId, string? version = null);
        Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId);
        Task<PluginInstallationResponse> RecordInstallationAsync(PluginInstallationRequest request);
        Task<MarketplacePluginsResponse> GetPluginsForMainApiAsync(int page = 1, int pageSize = 20, string? category = null, string? search = null);
        Task<List<MarketplaceCategory>> GetCategoriesForMainApiAsync();
    }
}
