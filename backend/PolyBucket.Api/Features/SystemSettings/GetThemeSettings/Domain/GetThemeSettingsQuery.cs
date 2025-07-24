using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain
{
    public class GetThemeSettingsQuery : IRequest<GetThemeSettingsResponse>
    {
    }

    public class GetThemeSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ThemeSettingsData? ThemeSettings { get; set; }
    }

    public class ThemeSettingsData
    {
        public string PrimaryColor { get; set; } = "#6366f1";
        public string PrimaryLightColor { get; set; } = "#818cf8";
        public string PrimaryDarkColor { get; set; } = "#4f46e5";
        public string SecondaryColor { get; set; } = "#8b5cf6";
        public string SecondaryLightColor { get; set; } = "#a78bfa";
        public string SecondaryDarkColor { get; set; } = "#7c3aed";
        public string AccentColor { get; set; } = "#06b6d4";
        public string AccentLightColor { get; set; } = "#22d3ee";
        public string AccentDarkColor { get; set; } = "#0891b2";
        public string BackgroundPrimaryColor { get; set; } = "#0f0f23";
        public string BackgroundSecondaryColor { get; set; } = "#1a1a2e";
        public string BackgroundTertiaryColor { get; set; } = "#16213e";
        public bool IsThemeCustomized { get; set; } = false;
    }
} 