using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Domain;

public interface IGetTokenSettingsService
{
    Task<TokenSettings> GetTokenSettingsAsync(CancellationToken cancellationToken = default);
}
