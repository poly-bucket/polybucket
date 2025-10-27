using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.Domain
{
    public interface IThemePlugin : IPlugin
    {
        string ThemeName { get; }
        Dictionary<string, string> CSSVariables { get; }
        Dictionary<string, object> ComponentOverrides { get; }
        Task ApplyThemeAsync();
        Task RemoveThemeAsync();
        Task<ThemeSettings> GetThemeSettingsAsync();
        Task UpdateThemeSettingsAsync(ThemeSettings settings);
    }

    public class ThemeSettings
    {
        public string PrimaryColor { get; set; } = "#007bff";
        public string SecondaryColor { get; set; } = "#6c757d";
        public string BackgroundColor { get; set; } = "#ffffff";
        public string SurfaceColor { get; set; } = "#f8f9fa";
        public string TextColor { get; set; } = "#212529";
        public string TextMutedColor { get; set; } = "#6c757d";
        public string BorderColor { get; set; } = "#dee2e6";
        public string ShadowColor { get; set; } = "rgba(0, 0, 0, 0.1)";
        public bool EnableAnimations { get; set; } = true;
        public string FontFamily { get; set; } = "system-ui, -apple-system, sans-serif";
        public int BorderRadius { get; set; } = 4;
    }

    public class ThemeApplicationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> AppliedVariables { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
