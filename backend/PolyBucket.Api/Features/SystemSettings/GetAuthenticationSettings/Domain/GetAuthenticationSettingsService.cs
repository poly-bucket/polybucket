using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Domain;

public class GetAuthenticationSettingsService(
    IGetAuthenticationSettingsRepository repository,
    ILogger<GetAuthenticationSettingsService> logger) : IGetAuthenticationSettingsService
{
    public async Task<AuthenticationSettings> GetAuthenticationSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.GetAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving authentication settings");
            return new AuthenticationSettings();
        }
    }
}
