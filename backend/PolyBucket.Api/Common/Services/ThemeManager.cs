using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace PolyBucket.Api.Common.Services
{
    public class ThemeManager(
        PolyBucketDbContext context,
        ILogger<ThemeManager> logger,
        IEnumerable<IThemePlugin> themePlugins) : IThemeManager
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<ThemeManager> _logger = logger;
        private readonly IEnumerable<IThemePlugin> _themePlugins = themePlugins;
        private readonly Dictionary<string, ThemeDefinition> _themeCache = new();
        private readonly Dictionary<string, ThemeComponents> _componentsCache = new();

        public async Task<IEnumerable<ThemeDefinition>> GetAllThemesAsync()
        {
            if (_themeCache.Count == 0)
            {
                await RefreshThemeCacheAsync();
            }

            return _themeCache.Values;
        }

        public async Task<ThemeDefinition?> GetThemeAsync(string themeId)
        {
            if (_themeCache.Count == 0)
            {
                await RefreshThemeCacheAsync();
            }

            return _themeCache.TryGetValue(themeId, out var theme) ? theme : null;
        }

        public async Task<ThemeDefinition?> GetActiveThemeAsync()
        {
            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync();
            if (systemSetup == null)
            {
                return null;
            }

            var activeThemeId = systemSetup.ActiveThemeId ?? "liquid-glass-default";
            return await GetThemeAsync(activeThemeId);
        }

        public async Task<bool> SetActiveThemeAsync(string themeId)
        {
            var theme = await GetThemeAsync(themeId);
            if (theme == null)
            {
                _logger.LogWarning("Attempted to set unknown theme: {ThemeId}", themeId);
                return false;
            }

            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync();
            if (systemSetup == null)
            {
                _logger.LogError("System setup not found");
                return false;
            }

            systemSetup.ActiveThemeId = themeId;
            systemSetup.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Active theme set to: {ThemeId}", themeId);
            return true;
        }

        public async Task<ThemeComponents?> GetActiveThemeComponentsAsync()
        {
            var activeTheme = await GetActiveThemeAsync();
            if (activeTheme == null)
            {
                return null;
            }

            return await GetThemeComponentsAsync(activeTheme.Id);
        }

        public async Task<ThemeComponents?> GetThemeComponentsAsync(string themeId)
        {
            if (_componentsCache.TryGetValue(themeId, out var cachedComponents))
            {
                return cachedComponents;
            }

            var theme = await GetThemeAsync(themeId);
            if (theme == null)
            {
                return null;
            }

            var plugin = _themePlugins.FirstOrDefault(p => p.GetThemes().Any(t => t.Id == themeId));
            if (plugin == null)
            {
                _logger.LogWarning("No plugin found for theme: {ThemeId}", themeId);
                return null;
            }

            try
            {
                var components = await plugin.GetThemeComponentsAsync(themeId);
                _componentsCache[themeId] = components;
                return components;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get theme components for: {ThemeId}", themeId);
                return null;
            }
        }

        public async Task<ThemeValidationResult> ValidateThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration)
        {
            var plugin = _themePlugins.FirstOrDefault(p => p.GetThemes().Any(t => t.Id == themeId));
            if (plugin == null)
            {
                return new ThemeValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"No plugin found for theme: {themeId}" }
                };
            }

            return plugin.ValidateThemeConfiguration(themeId, configuration);
        }

        public async Task<ThemeApplicationResult> ApplyThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration)
        {
            var plugin = _themePlugins.FirstOrDefault(p => p.GetThemes().Any(t => t.Id == themeId));
            if (plugin == null)
            {
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = $"No plugin found for theme: {themeId}"
                };
            }

            var result = await plugin.ApplyThemeConfigurationAsync(themeId, configuration);
            if (result.Success)
            {
                // Clear cache for this theme
                _componentsCache.Remove(themeId);
                
                // Save configuration to database
                await SaveThemeConfigurationAsync(themeId, configuration);
            }

            return result;
        }

        public async Task<Dictionary<string, object>> GetCurrentThemeConfigurationAsync()
        {
            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync();
            if (systemSetup == null)
            {
                return new Dictionary<string, object>();
            }

            if (string.IsNullOrEmpty(systemSetup.ThemeConfiguration))
            {
                // Return default configuration for active theme
                var activeTheme = await GetActiveThemeAsync();
                return activeTheme?.DefaultConfiguration ?? new Dictionary<string, object>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(systemSetup.ThemeConfiguration) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize theme configuration");
                return new Dictionary<string, object>();
            }
        }

        public async Task<ThemeApplicationResult> UpdateThemeConfigurationAsync(Dictionary<string, object> configuration)
        {
            var activeTheme = await GetActiveThemeAsync();
            if (activeTheme == null)
            {
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = "No active theme found"
                };
            }

            return await ApplyThemeConfigurationAsync(activeTheme.Id, configuration);
        }

        public async Task<ThemeApplicationResult> ResetThemeToDefaultAsync()
        {
            var activeTheme = await GetActiveThemeAsync();
            if (activeTheme == null)
            {
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = "No active theme found"
                };
            }

            return await ApplyThemeConfigurationAsync(activeTheme.Id, activeTheme.DefaultConfiguration);
        }

        public IEnumerable<IThemePlugin> GetThemePlugins()
        {
            return _themePlugins;
        }

        private async Task RefreshThemeCacheAsync()
        {
            _themeCache.Clear();
            _componentsCache.Clear();

            foreach (var plugin in _themePlugins)
            {
                try
                {
                    var themes = plugin.GetThemes();
                    foreach (var theme in themes)
                    {
                        _themeCache[theme.Id] = theme;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load themes from plugin: {PluginId}", plugin.Id);
                }
            }

            _logger.LogInformation("Theme cache refreshed. Loaded {ThemeCount} themes from {PluginCount} plugins", 
                _themeCache.Count, _themePlugins.Count());
        }

        private async Task SaveThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration)
        {
            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync();
            if (systemSetup == null)
            {
                _logger.LogError("System setup not found when saving theme configuration");
                return;
            }

            systemSetup.ActiveThemeId = themeId;
            systemSetup.ThemeConfiguration = JsonSerializer.Serialize(configuration);
            systemSetup.IsThemeCustomized = true;
            systemSetup.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Theme configuration saved for theme: {ThemeId}", themeId);
        }
    }
} 