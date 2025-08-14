using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Repository
{
    public interface IFontAwesomeSettingsRepository
    {
        Task<FontAwesomeSettings?> GetSettingsAsync();
        Task<FontAwesomeSettings> CreateSettingsAsync(FontAwesomeSettings settings);
        Task<FontAwesomeSettings> UpdateSettingsAsync(FontAwesomeSettings settings);
    }
}
