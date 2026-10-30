namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;

public interface IGetThemeSettingsService
{
    Task<GetThemeSettingsResponse> GetThemeSettingsAsync(CancellationToken cancellationToken = default);
}
