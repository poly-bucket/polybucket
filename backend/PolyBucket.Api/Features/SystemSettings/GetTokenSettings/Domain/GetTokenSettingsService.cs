using PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Domain;

public class GetTokenSettingsService(
    IGetTokenSettingsRepository repository,
    ILogger<GetTokenSettingsService> logger) : IGetTokenSettingsService
{
    public async Task<TokenSettings> GetTokenSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.GetAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting token settings");
            return new TokenSettings();
        }
    }
}
