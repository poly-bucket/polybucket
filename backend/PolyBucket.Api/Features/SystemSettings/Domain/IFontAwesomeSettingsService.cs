using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Domain
{
    public interface IFontAwesomeSettingsService
    {
        Task<FontAwesomeSettings> GetSettingsAsync();
        Task<FontAwesomeSettings> UpdateSettingsAsync(FontAwesomeSettings settings);
        Task<bool> TestLicenseKeyAsync(string licenseKey);
        Task<bool> IsProEnabledAsync();
    }
}
