using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services
{
    public class ThemePluginService
    {
        private readonly ILogger<ThemePluginService> _logger;
        private readonly Dictionary<string, IThemePlugin> _activeThemes = new();

        public ThemePluginService(ILogger<ThemePluginService> logger)
        {
            _logger = logger;
        }

        public async Task<ThemeApplicationResult> ApplyThemeAsync(IThemePlugin themePlugin)
        {
            try
            {
                _logger.LogInformation("Applying theme {ThemeName} from plugin {PluginId}", 
                    themePlugin.ThemeName, themePlugin.Id);

                // Store theme settings
                var settings = await themePlugin.GetThemeSettingsAsync();
                
                // Apply CSS variables
                var appliedVariables = new Dictionary<string, string>();
                foreach (var variable in themePlugin.CSSVariables)
                {
                    appliedVariables[variable.Key] = variable.Value;
                }

                // Store active theme
                _activeThemes[themePlugin.Id] = themePlugin;

                await themePlugin.ApplyThemeAsync();

                _logger.LogInformation("Successfully applied theme {ThemeName}", themePlugin.ThemeName);

                return new ThemeApplicationResult
                {
                    Success = true,
                    Message = $"Theme {themePlugin.ThemeName} applied successfully",
                    AppliedVariables = appliedVariables
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme {ThemeName}", themePlugin.ThemeName);
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = $"Failed to apply theme: {ex.Message}",
                    Errors = new() { ex.Message }
                };
            }
        }

        public async Task<ThemeApplicationResult> RemoveThemeAsync(string pluginId)
        {
            try
            {
                if (!_activeThemes.TryGetValue(pluginId, out var themePlugin))
                {
                    return new ThemeApplicationResult
                    {
                        Success = false,
                        Message = $"Theme from plugin {pluginId} is not currently active",
                        Errors = new() { "Theme not active" }
                    };
                }

                _logger.LogInformation("Removing theme {ThemeName} from plugin {PluginId}", 
                    themePlugin.ThemeName, pluginId);

                await themePlugin.RemoveThemeAsync();
                _activeThemes.Remove(pluginId);

                _logger.LogInformation("Successfully removed theme {ThemeName}", themePlugin.ThemeName);

                return new ThemeApplicationResult
                {
                    Success = true,
                    Message = $"Theme {themePlugin.ThemeName} removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing theme from plugin {PluginId}", pluginId);
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = $"Failed to remove theme: {ex.Message}",
                    Errors = new() { ex.Message }
                };
            }
        }

        public async Task<ThemeApplicationResult> UpdateThemeSettingsAsync(string pluginId, ThemeSettings settings)
        {
            try
            {
                if (!_activeThemes.TryGetValue(pluginId, out var themePlugin))
                {
                    return new ThemeApplicationResult
                    {
                        Success = false,
                        Message = $"Theme from plugin {pluginId} is not currently active",
                        Errors = new() { "Theme not active" }
                    };
                }

                _logger.LogInformation("Updating theme settings for plugin {PluginId}", pluginId);

                await themePlugin.UpdateThemeSettingsAsync(settings);
                
                // Reapply theme with new settings
                var result = await ApplyThemeAsync(themePlugin);

                _logger.LogInformation("Successfully updated theme settings for plugin {PluginId}", pluginId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme settings for plugin {PluginId}", pluginId);
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = $"Failed to update theme settings: {ex.Message}",
                    Errors = new() { ex.Message }
                };
            }
        }

        public List<ActiveTheme> GetActiveThemes()
        {
            var activeThemes = new List<ActiveTheme>();
            
            foreach (var kvp in _activeThemes)
            {
                activeThemes.Add(new ActiveTheme
                {
                    PluginId = kvp.Key,
                    ThemeName = kvp.Value.ThemeName,
                    CSSVariables = kvp.Value.CSSVariables,
                    ComponentOverrides = kvp.Value.ComponentOverrides
                });
            }

            return activeThemes;
        }

        public bool IsThemeActive(string pluginId)
        {
            return _activeThemes.ContainsKey(pluginId);
        }
    }

    public class ActiveTheme
    {
        public string PluginId { get; set; } = string.Empty;
        public string ThemeName { get; set; } = string.Empty;
        public Dictionary<string, string> CSSVariables { get; set; } = new();
        public Dictionary<string, object> ComponentOverrides { get; set; } = new();
    }
}
