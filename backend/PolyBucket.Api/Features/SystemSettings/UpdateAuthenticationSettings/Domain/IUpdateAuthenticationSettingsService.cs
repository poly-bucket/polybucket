using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Domain;

public interface IUpdateAuthenticationSettingsService
{
    Task<bool> UpdateAuthenticationSettingsAsync(AuthenticationSettings settings, CancellationToken cancellationToken = default);
}
