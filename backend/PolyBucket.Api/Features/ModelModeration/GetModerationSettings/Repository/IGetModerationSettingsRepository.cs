using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Repository;

public interface IGetModerationSettingsRepository
{
    Task<GetModerationSettingsResponseDto> GetSettingsAsync(CancellationToken cancellationToken = default);
}
