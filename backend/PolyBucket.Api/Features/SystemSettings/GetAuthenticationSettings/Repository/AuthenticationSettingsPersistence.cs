using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Repository;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Repository;

public class AuthenticationSettingsPersistence(
    PolyBucketDbContext context,
    IEmailSettingsResolver emailSettingsResolver) : IGetAuthenticationSettingsRepository, IUpdateAuthenticationSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;
    private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;

    public async Task<AuthenticationSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.SystemSettings
            .Where(s => s.Key.StartsWith("Auth:"))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var authSettings = new AuthenticationSettings();

        if (settings.TryGetValue(SystemSettingKeys.AuthLoginMethod, out var loginMethodStr)
            && Enum.TryParse<LoginMethod>(loginMethodStr, true, out var loginMethod))
        {
            authSettings.LoginMethod = loginMethod;
        }

        if (settings.TryGetValue(SystemSettingKeys.AuthAllowEmailLogin, out var allowEmailStr))
        {
            authSettings.AllowEmailLogin = bool.TryParse(allowEmailStr, out var allowEmail) && allowEmail;
        }

        if (settings.TryGetValue(SystemSettingKeys.AuthAllowUsernameLogin, out var allowUsernameStr))
        {
            authSettings.AllowUsernameLogin = bool.TryParse(allowUsernameStr, out var allowUsername) && allowUsername;
        }

        var emailSettings = await _emailSettingsResolver.GetEffectiveSettingsAsync();
        authSettings.RequireEmailVerification = emailSettings.CanDeliver && emailSettings.RequireEmailVerification;

        var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup != null)
        {
            authSettings.MaxFailedLoginAttempts = systemSetup.MaxFailedLoginAttempts;
            authSettings.LockoutDurationMinutes = systemSetup.LockoutDurationMinutes;
            authSettings.RequireStrongPasswords = systemSetup.RequireStrongPasswords;
            authSettings.PasswordMinLength = systemSetup.PasswordMinLength;
        }

        return authSettings;
    }

    public async Task<bool> UpdateAsync(AuthenticationSettings settings, CancellationToken cancellationToken = default)
    {
        var settingsToUpdate = new Dictionary<string, string>
        {
            { SystemSettingKeys.AuthLoginMethod, settings.LoginMethod.ToString() },
            { SystemSettingKeys.AuthAllowEmailLogin, settings.AllowEmailLogin.ToString().ToLower() },
            { SystemSettingKeys.AuthAllowUsernameLogin, settings.AllowUsernameLogin.ToString().ToLower() }
        };

        var existingSettings = await _context.SystemSettings
            .Where(s => settingsToUpdate.Keys.Contains(s.Key))
            .ToListAsync(cancellationToken);

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

        var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup != null)
        {
            systemSetup.MaxFailedLoginAttempts = settings.MaxFailedLoginAttempts;
            systemSetup.LockoutDurationMinutes = settings.LockoutDurationMinutes;
            systemSetup.RequireStrongPasswords = settings.RequireStrongPasswords;
            systemSetup.PasswordMinLength = settings.PasswordMinLength;
            systemSetup.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
