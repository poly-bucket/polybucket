using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Domain;

public interface IGetAuthenticationSettingsService
{
    Task<AuthenticationSettings> GetAuthenticationSettingsAsync(CancellationToken cancellationToken = default);
}
