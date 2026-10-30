using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Domain;

public class UpdateTokenSettingsService(
    IUpdateTokenSettingsRepository repository,
    ILogger<UpdateTokenSettingsService> logger) : IUpdateTokenSettingsService
{
    public async Task<bool> UpdateTokenSettingsAsync(TokenSettings settings, CancellationToken cancellationToken = default)
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

            await repository.UpdateAsync(settings, cancellationToken);
            logger.LogInformation("Token settings updated successfully");
            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating token settings");
            return false;
        }
    }
}
