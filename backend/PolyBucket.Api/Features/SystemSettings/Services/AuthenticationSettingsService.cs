using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public class AuthenticationSettingsService(
    PolyBucketDbContext context,
    ILogger<AuthenticationSettingsService> logger) : IAuthenticationSettingsService
{
    private readonly PolyBucketDbContext _context = context;
    private readonly ILogger<AuthenticationSettingsService> _logger = logger;

    public async Task<AuthenticationSettings> GetAuthenticationSettingsAsync()
    {
        try
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Key.StartsWith("Auth:"))
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            var authSettings = new AuthenticationSettings();

            // Parse login method
            if (settings.TryGetValue(SystemSettingKeys.AuthLoginMethod, out var loginMethodStr))
            {
                if (Enum.TryParse<LoginMethod>(loginMethodStr, true, out var loginMethod))
                {
                    authSettings.LoginMethod = loginMethod;
                }
            }

            // Parse boolean settings
            if (settings.TryGetValue(SystemSettingKeys.AuthAllowEmailLogin, out var allowEmailStr))
            {
                authSettings.AllowEmailLogin = bool.TryParse(allowEmailStr, out var allowEmail) && allowEmail;
            }

            if (settings.TryGetValue(SystemSettingKeys.AuthAllowUsernameLogin, out var allowUsernameStr))
            {
                authSettings.AllowUsernameLogin = bool.TryParse(allowUsernameStr, out var allowUsername) && allowUsername;
            }

            // Get other settings from SystemSetup
            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync();
            if (systemSetup != null)
            {
                authSettings.RequireEmailVerification = systemSetup.RequireEmailVerification;
                authSettings.MaxFailedLoginAttempts = systemSetup.MaxFailedLoginAttempts;
                authSettings.LockoutDurationMinutes = systemSetup.LockoutDurationMinutes;
                authSettings.RequireStrongPasswords = systemSetup.RequireStrongPasswords;
                authSettings.PasswordMinLength = systemSetup.PasswordMinLength;
            }

            return authSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving authentication settings");
            return new AuthenticationSettings(); // Return default settings
        }
    }

    public async Task<bool> UpdateAuthenticationSettingsAsync(AuthenticationSettings settings)
    {
        try
        {
            if (!settings.IsValid())
            {
                _logger.LogWarning("Invalid authentication settings provided");
                return false;
            }

            var settingsToUpdate = new Dictionary<string, string>
            {
                { SystemSettingKeys.AuthLoginMethod, settings.LoginMethod.ToString() },
                { SystemSettingKeys.AuthAllowEmailLogin, settings.AllowEmailLogin.ToString().ToLower() },
                { SystemSettingKeys.AuthAllowUsernameLogin, settings.AllowUsernameLogin.ToString().ToLower() }
            };

            // Get existing settings
            var existingSettings = await _context.SystemSettings
                .Where(s => settingsToUpdate.Keys.Contains(s.Key))
                .ToListAsync();

            // Update or create settings
            foreach (var settingPair in settingsToUpdate)
            {
                var existing = existingSettings.FirstOrDefault(s => s.Key == settingPair.Key);
                if (existing != null)
                {
                    existing.Value = settingPair.Value;
                }
                else
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        Key = settingPair.Key,
                        Value = settingPair.Value
                    });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Authentication settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update authentication settings");
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