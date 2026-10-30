using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public class AuthenticationSettingsService(
    IGetAuthenticationSettingsRepository getAuthenticationSettingsRepository,
    IUpdateAuthenticationSettingsRepository updateAuthenticationSettingsRepository,
    ILogger<AuthenticationSettingsService> logger) : IAuthenticationSettingsService
{
    public async Task<AuthenticationSettings> GetAuthenticationSettingsAsync()
    {
        try
        {
            return await getAuthenticationSettingsRepository.GetAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving authentication settings");
            return new AuthenticationSettings();
        }
    }

    public async Task<bool> UpdateAuthenticationSettingsAsync(AuthenticationSettings settings)
    {
        try
        {
            if (!settings.IsValid())
            {
                logger.LogWarning("Invalid authentication settings provided");
                return false;
            }

            await updateAuthenticationSettingsRepository.UpdateAsync(settings);
            logger.LogInformation("Authentication settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update authentication settings");
            return false;
        }
    }

    public async Task<bool> IsEmailLoginEnabledAsync()
    {
        var settings = await GetAuthenticationSettingsAsync();
        return settings.AllowEmailLogin;
    }

    public async Task<bool> IsUsernameLoginEnabledAsync()
    {
        var settings = await GetAuthenticationSettingsAsync();
        return settings.AllowUsernameLogin;
    }

    public async Task<LoginMethod> GetLoginMethodAsync()
    {
        var settings = await GetAuthenticationSettingsAsync();
        return settings.LoginMethod;
    }
}
