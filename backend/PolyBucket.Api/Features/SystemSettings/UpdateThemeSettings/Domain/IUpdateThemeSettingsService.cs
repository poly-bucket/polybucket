namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;

public interface IUpdateThemeSettingsService
{
    Task<UpdateThemeSettingsResponse> UpdateThemeSettingsAsync(UpdateThemeSettingsCommand command, CancellationToken cancellationToken = default);
}
