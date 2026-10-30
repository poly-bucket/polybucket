using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;

public class UpdateThemeSettingsService(
    IUpdateThemeSettingsRepository repository,
    ILogger<UpdateThemeSettingsService> logger) : IUpdateThemeSettingsService
{
    public async Task<UpdateThemeSettingsResponse> UpdateThemeSettingsAsync(UpdateThemeSettingsCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var systemSetup = await repository.GetSystemSetupAsync(cancellationToken);
            if (systemSetup == null)
            {
                return new UpdateThemeSettingsResponse
                {
                    Success = false,
                    Message = "System setup not found. Please complete first-time setup first."
                };
            }

            systemSetup.PrimaryColor = request.PrimaryColor;
            systemSetup.PrimaryLightColor = request.PrimaryLightColor;
            systemSetup.PrimaryDarkColor = request.PrimaryDarkColor;
            systemSetup.SecondaryColor = request.SecondaryColor;
            systemSetup.SecondaryLightColor = request.SecondaryLightColor;
            systemSetup.SecondaryDarkColor = request.SecondaryDarkColor;
            systemSetup.AccentColor = request.AccentColor;
            systemSetup.AccentLightColor = request.AccentLightColor;
            systemSetup.AccentDarkColor = request.AccentDarkColor;
            systemSetup.BackgroundPrimaryColor = request.BackgroundPrimaryColor;
            systemSetup.BackgroundSecondaryColor = request.BackgroundSecondaryColor;
            systemSetup.BackgroundTertiaryColor = request.BackgroundTertiaryColor;
            systemSetup.IsThemeCustomized = true;
            systemSetup.UpdatedAt = DateTime.UtcNow;

            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Theme settings updated successfully");

            return new UpdateThemeSettingsResponse
            {
                Success = true,
                Message = "Theme settings updated successfully",
                IsThemeCustomized = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update theme settings");
            return new UpdateThemeSettingsResponse
            {
                Success = false,
                Message = "Failed to update theme settings. Please try again."
            };
        }
    }
}
