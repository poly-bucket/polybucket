using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Plugins
{
    /// <summary>
    /// Interface for plugins that provide theme functionality
    /// </summary>
    public interface IThemePlugin : IPlugin
    {
        /// <summary>
        /// Get all themes provided by this plugin
        /// </summary>
        IEnumerable<ThemeDefinition> GetThemes();
        
        /// <summary>
        /// Get theme components (CSS, JS, etc.) for a specific theme
        /// </summary>
        /// <param name="themeId">The theme identifier</param>
        /// <returns>Theme components</returns>
        Task<ThemeComponents> GetThemeComponentsAsync(string themeId);
        
        /// <summary>
        /// Validate theme configuration
        /// </summary>
        /// <param name="themeId">The theme identifier</param>
        /// <param name="configuration">Theme configuration</param>
        /// <returns>Validation result</returns>
        ThemeValidationResult ValidateThemeConfiguration(string themeId, Dictionary<string, object> configuration);
        
        /// <summary>
        /// Apply theme configuration
        /// </summary>
        /// <param name="themeId">The theme identifier</param>
        /// <param name="configuration">Theme configuration</param>
        /// <returns>Application result</returns>
        Task<ThemeApplicationResult> ApplyThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration);
    }

    /// <summary>
    /// Definition of a theme provided by a plugin
    /// </summary>
    public class ThemeDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string PreviewImageUrl { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public bool IsCustomizable { get; set; } = true;
        public List<ThemeCategory> Categories { get; set; } = new();
        public Dictionary<string, ThemeSetting> Settings { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public Dictionary<string, object> DefaultConfiguration { get; set; } = new();
    }

    /// <summary>
    /// Theme category for organization
    /// </summary>
    public class ThemeCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
    }

    /// <summary>
    /// Theme setting definition
    /// </summary>
    public class ThemeSetting
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ThemeSettingType Type { get; set; }
        public object DefaultValue { get; set; } = string.Empty;
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
        public List<string> Options { get; set; } = new();
        public bool Required { get; set; } = false;
        public string? ValidationPattern { get; set; }
        public string? ValidationMessage { get; set; }
    }

    /// <summary>
    /// Theme setting types
    /// </summary>
    public enum ThemeSettingType
    {
        Color,
        Number,
        Boolean,
        Select,
        MultiSelect,
        Text,
        TextArea,
        Range,
        File,
        Json
    }

    /// <summary>
    /// Theme components (CSS, JS, etc.)
    /// </summary>
    public class ThemeComponents
    {
        public string Css { get; set; } = string.Empty;
        public string JavaScript { get; set; } = string.Empty;
        public Dictionary<string, string> Assets { get; set; } = new();
        public List<string> ExternalCssUrls { get; set; } = new();
        public List<string> ExternalJsUrls { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
    }

    /// <summary>
    /// Theme validation result
    /// </summary>
    public class ThemeValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Theme application result
    /// </summary>
    public class ThemeApplicationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> AppliedConfiguration { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
} 