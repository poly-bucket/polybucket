using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Repository;

public interface IGetAuthenticationSettingsRepository
{
    Task<AuthenticationSettings> GetAsync(CancellationToken cancellationToken = default);
}
