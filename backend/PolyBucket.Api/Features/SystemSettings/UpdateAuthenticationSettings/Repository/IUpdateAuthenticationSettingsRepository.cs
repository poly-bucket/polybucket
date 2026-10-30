using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Repository;

public interface IUpdateAuthenticationSettingsRepository
{
    Task<bool> UpdateAsync(AuthenticationSettings settings, CancellationToken cancellationToken = default);
}
