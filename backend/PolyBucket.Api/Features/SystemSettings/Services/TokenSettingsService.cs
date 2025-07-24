using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

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
    public int AccessTokenExpiryHours { get; set; } = 1; // Default: 1 hour
    public int RefreshTokenExpiryDays { get; set; } = 7; // Default: 7 days
    public bool EnableRefreshTokens { get; set; } = true; // Default: enabled
}

public class TokenSettingsService(
    PolyBucketDbContext context,
    ILogger<TokenSettingsService> logger) : ITokenSettingsService
{
    private readonly PolyBucketDbContext _context = context;
    private readonly ILogger<TokenSettingsService> _logger = logger;

    public async Task<TokenSettings> GetTokenSettingsAsync()
    {
        try
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Key.StartsWith("Token:"))
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            var tokenSettings = new TokenSettings();

            // Parse access token expiry hours
            if (settings.TryGetValue(SystemSettingKeys.TokenAccessTokenExpiryHours, out var accessTokenHoursStr))
            {
                if (int.TryParse(accessTokenHoursStr, out var accessTokenHours) && accessTokenHours > 0)
                {
                    tokenSettings.AccessTokenExpiryHours = accessTokenHours;
                }
            }

            // Parse refresh token expiry days
            if (settings.TryGetValue(SystemSettingKeys.TokenRefreshTokenExpiryDays, out var refreshTokenDaysStr))
            {
                if (int.TryParse(refreshTokenDaysStr, out var refreshTokenDays) && refreshTokenDays > 0)
                {
                    tokenSettings.RefreshTokenExpiryDays = refreshTokenDays;
                }
            }

            // Parse enable refresh tokens
            if (settings.TryGetValue(SystemSettingKeys.TokenEnableRefreshTokens, out var enableRefreshTokensStr))
            {
                tokenSettings.EnableRefreshTokens = bool.TryParse(enableRefreshTokensStr, out var enableRefreshTokens) && enableRefreshTokens;
            }

            return tokenSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token settings");
            return new TokenSettings(); // Return defaults
        }
    }

    public async Task<bool> UpdateTokenSettingsAsync(TokenSettings settings)
    {
        try
        {
            // Validate settings
            if (settings.AccessTokenExpiryHours <= 0 || settings.AccessTokenExpiryHours > 24)
            {
                throw new ArgumentException("Access token expiry hours must be between 1 and 24");
            }

            if (settings.RefreshTokenExpiryDays <= 0 || settings.RefreshTokenExpiryDays > 365)
            {
                throw new ArgumentException("Refresh token expiry days must be between 1 and 365");
            }

            // Update or create settings
            var existingSettings = await _context.SystemSettings
                .Where(s => s.Key.StartsWith("Token:"))
                .ToListAsync();

            // Update access token expiry hours
            var accessTokenSetting = existingSettings.FirstOrDefault(s => s.Key == SystemSettingKeys.TokenAccessTokenExpiryHours);
            if (accessTokenSetting != null)
            {
                accessTokenSetting.Value = settings.AccessTokenExpiryHours.ToString();
            }
            else
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    Key = SystemSettingKeys.TokenAccessTokenExpiryHours,
                    Value = settings.AccessTokenExpiryHours.ToString()
                });
            }

            // Update refresh token expiry days
            var refreshTokenSetting = existingSettings.FirstOrDefault(s => s.Key == SystemSettingKeys.TokenRefreshTokenExpiryDays);
            if (refreshTokenSetting != null)
            {
                refreshTokenSetting.Value = settings.RefreshTokenExpiryDays.ToString();
            }
            else
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    Key = SystemSettingKeys.TokenRefreshTokenExpiryDays,
                    Value = settings.RefreshTokenExpiryDays.ToString()
                });
            }

            // Update enable refresh tokens
            var enableRefreshTokensSetting = existingSettings.FirstOrDefault(s => s.Key == SystemSettingKeys.TokenEnableRefreshTokens);
            if (enableRefreshTokensSetting != null)
            {
                enableRefreshTokensSetting.Value = settings.EnableRefreshTokens.ToString();
            }
            else
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    Key = SystemSettingKeys.TokenEnableRefreshTokens,
                    Value = settings.EnableRefreshTokens.ToString()
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Token settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating token settings");
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