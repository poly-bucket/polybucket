using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Domain;

public interface IUpdateTokenSettingsService
{
    Task<bool> UpdateTokenSettingsAsync(TokenSettings settings, CancellationToken cancellationToken = default);
}
