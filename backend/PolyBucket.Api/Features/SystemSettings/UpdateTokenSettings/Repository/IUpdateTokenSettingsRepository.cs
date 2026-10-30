using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Repository;

public interface IUpdateTokenSettingsRepository
{
    Task UpdateAsync(TokenSettings settings, CancellationToken cancellationToken = default);
}
