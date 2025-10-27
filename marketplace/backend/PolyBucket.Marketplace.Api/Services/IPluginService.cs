using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services
{
    public interface IPluginService
    {
        Task<List<Plugin>> GetPluginsAsync(string? category = null, string? search = null);
        Task<Plugin?> GetPluginAsync(string id);
        Task<Plugin> CreatePluginAsync(Plugin plugin);
        Task<Plugin> UpdatePluginAsync(Plugin plugin);
        Task<bool> DeletePluginAsync(string id);
        Task<List<string>> GetCategoriesAsync();
    }
}
