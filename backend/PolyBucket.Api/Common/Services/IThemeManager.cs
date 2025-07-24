using PolyBucket.Api.Common.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Services
{
    /// <summary>
    /// Service for managing themes from plugins
    /// </summary>
    public interface IThemeManager
    {
        /// <summary>
        /// Get all available themes from all theme plugins
        /// </summary>
        /// <returns>List of all available themes</returns>
        Task<IEnumerable<ThemeDefinition>> GetAllThemesAsync();
        
        /// <summary>
        /// Get a specific theme by ID
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <returns>Theme definition or null if not found</returns>
        Task<ThemeDefinition?> GetThemeAsync(string themeId);
        
        /// <summary>
        /// Get the currently active theme
        /// </summary>
        /// <returns>Currently active theme</returns>
        Task<ThemeDefinition?> GetActiveThemeAsync();
        
        /// <summary>
        /// Set the active theme
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <returns>Success result</returns>
        Task<bool> SetActiveThemeAsync(string themeId);
        
        /// <summary>
        /// Get theme components for the active theme
        /// </summary>
        /// <returns>Theme components (CSS, JS, etc.)</returns>
        Task<ThemeComponents?> GetActiveThemeComponentsAsync();
        
        /// <summary>
        /// Get theme components for a specific theme
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <returns>Theme components</returns>
        Task<ThemeComponents?> GetThemeComponentsAsync(string themeId);
        
        /// <summary>
        /// Validate theme configuration
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <param name="configuration">Theme configuration</param>
        /// <returns>Validation result</returns>
        Task<ThemeValidationResult> ValidateThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration);
        
        /// <summary>
        /// Apply theme configuration
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <param name="configuration">Theme configuration</param>
        /// <returns>Application result</returns>
        Task<ThemeApplicationResult> ApplyThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration);
        
        /// <summary>
        /// Get current theme configuration
        /// </summary>
        /// <returns>Current theme configuration</returns>
        Task<Dictionary<string, object>> GetCurrentThemeConfigurationAsync();
        
        /// <summary>
        /// Update theme configuration
        /// </summary>
        /// <param name="configuration">New theme configuration</param>
        /// <returns>Update result</returns>
        Task<ThemeApplicationResult> UpdateThemeConfigurationAsync(Dictionary<string, object> configuration);
        
        /// <summary>
        /// Reset theme to default configuration
        /// </summary>
        /// <returns>Reset result</returns>
        Task<ThemeApplicationResult> ResetThemeToDefaultAsync();
        
        /// <summary>
        /// Get theme plugins
        /// </summary>
        /// <returns>List of theme plugins</returns>
        IEnumerable<IThemePlugin> GetThemePlugins();
    }
} 