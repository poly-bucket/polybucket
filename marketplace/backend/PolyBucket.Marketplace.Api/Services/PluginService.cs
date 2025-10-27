using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services
{
    public class PluginService : IPluginService
    {
        private readonly MarketplaceDbContext _context;
        private readonly ILogger<PluginService> _logger;

        public PluginService(MarketplaceDbContext context, ILogger<PluginService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Plugin>> GetPluginsAsync(string? category = null, string? search = null)
        {
            try
            {
                var query = _context.Plugins
                    .Include(p => p.Versions)
                    .Where(p => p.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
                }

                var plugins = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} plugins", plugins.Count);
                return plugins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plugins");
                return new List<Plugin>();
            }
        }

        public async Task<Plugin?> GetPluginAsync(string id)
        {
            try
            {
                var plugin = await _context.Plugins
                    .Include(p => p.Versions)
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (plugin != null)
                {
                    _logger.LogInformation("Retrieved plugin: {PluginName}", plugin.Name);
                }

                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plugin: {PluginId}", id);
                return null;
            }
        }

        public async Task<Plugin> CreatePluginAsync(Plugin plugin)
        {
            try
            {
                plugin.Id = Guid.NewGuid().ToString();
                plugin.CreatedAt = DateTime.UtcNow;
                plugin.UpdatedAt = DateTime.UtcNow;
                plugin.IsActive = true;

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created plugin: {PluginName} with ID: {PluginId}", plugin.Name, plugin.Id);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin: {PluginName}", plugin.Name);
                throw;
            }
        }

        public async Task<Plugin> UpdatePluginAsync(Plugin plugin)
        {
            try
            {
                plugin.UpdatedAt = DateTime.UtcNow;
                _context.Plugins.Update(plugin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated plugin: {PluginName} with ID: {PluginId}", plugin.Name, plugin.Id);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plugin: {PluginId}", plugin.Id);
                throw;
            }
        }

        public async Task<bool> DeletePluginAsync(string id)
        {
            try
            {
                var plugin = await _context.Plugins.FindAsync(id);
                if (plugin == null)
                {
                    return false;
                }

                plugin.IsActive = false;
                plugin.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted plugin: {PluginName} with ID: {PluginId}", plugin.Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plugin: {PluginId}", id);
                return false;
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Plugins
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return new List<string>();
            }
        }
    }
}
