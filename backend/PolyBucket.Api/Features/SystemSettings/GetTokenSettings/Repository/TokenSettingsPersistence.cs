using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Repository;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Repository;

public class TokenSettingsPersistence(PolyBucketDbContext context)
    : IGetTokenSettingsRepository, IUpdateTokenSettingsRepository
{
    public async Task<TokenSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await context.SystemSettings
            .Where(s => s.Key.StartsWith("Token:"))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var tokenSettings = new TokenSettings();

        if (settings.TryGetValue(SystemSettingKeys.TokenAccessTokenExpiryHours, out var accessTokenHoursStr)
            && int.TryParse(accessTokenHoursStr, out var accessTokenHours) && accessTokenHours > 0)
        {
            tokenSettings.AccessTokenExpiryHours = accessTokenHours;
        }

        if (settings.TryGetValue(SystemSettingKeys.TokenRefreshTokenExpiryDays, out var refreshTokenDaysStr)
            && int.TryParse(refreshTokenDaysStr, out var refreshTokenDays) && refreshTokenDays > 0)
        {
            tokenSettings.RefreshTokenExpiryDays = refreshTokenDays;
        }

        if (settings.TryGetValue(SystemSettingKeys.TokenEnableRefreshTokens, out var enableRefreshTokensStr))
        {
            tokenSettings.EnableRefreshTokens = bool.TryParse(enableRefreshTokensStr, out var enableRefreshTokens) && enableRefreshTokens;
        }

        return tokenSettings;
    }

    public async Task UpdateAsync(TokenSettings settings, CancellationToken cancellationToken = default)
    {
        var existingSettings = await context.SystemSettings
            .Where(s => s.Key.StartsWith("Token:"))
            .ToListAsync(cancellationToken);

        Upsert(existingSettings, SystemSettingKeys.TokenAccessTokenExpiryHours, settings.AccessTokenExpiryHours.ToString());
        Upsert(existingSettings, SystemSettingKeys.TokenRefreshTokenExpiryDays, settings.RefreshTokenExpiryDays.ToString());
        Upsert(existingSettings, SystemSettingKeys.TokenEnableRefreshTokens, settings.EnableRefreshTokens.ToString());

        await context.SaveChangesAsync(cancellationToken);
    }

    private void Upsert(List<SystemSetting> existingSettings, string key, string value)
    {
        var existing = existingSettings.FirstOrDefault(s => s.Key == key);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }
    }
}
