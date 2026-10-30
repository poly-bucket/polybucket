using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Repository;

public interface IGetTokenSettingsRepository
{
    Task<TokenSettings> GetAsync(CancellationToken cancellationToken = default);
}
