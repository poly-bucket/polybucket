using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;

public class GetThemeSettingsService(
    IGetThemeSettingsRepository repository,
    ILogger<GetThemeSettingsService> logger) : IGetThemeSettingsService
{
    public async Task<GetThemeSettingsResponse> GetThemeSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var systemSetup = await repository.GetSystemSetupAsync(cancellationToken);
            if (systemSetup == null)
            {
                return new GetThemeSettingsResponse
                {
                    Success = false,
                    Message = "System setup not found. Please complete first-time setup first."
                };
            }

            return new GetThemeSettingsResponse
            {
                Success = true,
                Message = "Theme settings retrieved successfully",
                ThemeSettings = new ThemeSettingsData
                {
                    PrimaryColor = systemSetup.PrimaryColor,
                    PrimaryLightColor = systemSetup.PrimaryLightColor,
                    PrimaryDarkColor = systemSetup.PrimaryDarkColor,
                    SecondaryColor = systemSetup.SecondaryColor,
                    SecondaryLightColor = systemSetup.SecondaryLightColor,
                    SecondaryDarkColor = systemSetup.SecondaryDarkColor,
                    AccentColor = systemSetup.AccentColor,
                    AccentLightColor = systemSetup.AccentLightColor,
                    AccentDarkColor = systemSetup.AccentDarkColor,
                    BackgroundPrimaryColor = systemSetup.BackgroundPrimaryColor,
                    BackgroundSecondaryColor = systemSetup.BackgroundSecondaryColor,
                    BackgroundTertiaryColor = systemSetup.BackgroundTertiaryColor,
                    IsThemeCustomized = systemSetup.IsThemeCustomized
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve theme settings");
            return new GetThemeSettingsResponse
            {
                Success = false,
                Message = "Failed to retrieve theme settings. Please try again."
            };
        }
    }
}
