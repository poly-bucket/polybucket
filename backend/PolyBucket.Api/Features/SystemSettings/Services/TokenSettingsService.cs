using PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ITokenSettingsService
{
    Task<TokenSettings> GetTokenSettingsAsync();
    Task<bool> UpdateTokenSettingsAsync(TokenSettings settings);
    Task<int> GetAccessTokenExpiryMinutesAsync();
    Task<int> GetRefreshTokenExpiryDaysAsync();
    Task<bool> IsRefreshTokensEnabledAsync();
}

public class TokenSettings
{
    public int AccessTokenExpiryHours { get; set; } = 1;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public bool EnableRefreshTokens { get; set; } = true;
}

public class TokenSettingsService(
    IGetTokenSettingsRepository getTokenSettingsRepository,
    IUpdateTokenSettingsRepository updateTokenSettingsRepository,
    ILogger<TokenSettingsService> logger) : ITokenSettingsService
{
    public async Task<TokenSettings> GetTokenSettingsAsync()
    {
        try
        {
            return await getTokenSettingsRepository.GetAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting token settings");
            return new TokenSettings();
        }
    }

    public async Task<bool> UpdateTokenSettingsAsync(TokenSettings settings)
    {
        try
        {
            if (settings.AccessTokenExpiryHours <= 0 || settings.AccessTokenExpiryHours > 24)
            {
                throw new ArgumentException("Access token expiry hours must be between 1 and 24");
            }

            if (settings.RefreshTokenExpiryDays <= 0 || settings.RefreshTokenExpiryDays > 365)
            {
                throw new ArgumentException("Refresh token expiry days must be between 1 and 365");
            }

            await updateTokenSettingsRepository.UpdateAsync(settings);
            logger.LogInformation("Token settings updated successfully");
            return true;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            logger.LogError(ex, "Error updating token settings");
            return false;
        }
    }

    public async Task<int> GetAccessTokenExpiryMinutesAsync()
    {
        var settings = await GetTokenSettingsAsync();
        return settings.AccessTokenExpiryHours * 60;
    }

    public async Task<int> GetRefreshTokenExpiryDaysAsync()
    {
        var settings = await GetTokenSettingsAsync();
        return settings.RefreshTokenExpiryDays;
    }

    public async Task<bool> IsRefreshTokensEnabledAsync()
    {
        var settings = await GetTokenSettingsAsync();
        return settings.EnableRefreshTokens;
    }
}
