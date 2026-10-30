using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Domain;

public class UpdateAuthenticationSettingsService(
    IUpdateAuthenticationSettingsRepository repository,
    ILogger<UpdateAuthenticationSettingsService> logger) : IUpdateAuthenticationSettingsService
{
    public async Task<bool> UpdateAuthenticationSettingsAsync(AuthenticationSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!settings.IsValid())
            {
                logger.LogWarning("Invalid authentication settings provided");
                return false;
            }

            await repository.UpdateAsync(settings, cancellationToken);
            logger.LogInformation("Authentication settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update authentication settings");
            return false;
        }
    }
}
