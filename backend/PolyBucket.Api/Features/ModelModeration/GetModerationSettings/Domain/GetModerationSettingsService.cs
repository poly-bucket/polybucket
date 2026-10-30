using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Repository;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;

public interface IGetModerationSettingsService
{
    Task<GetModerationSettingsResponseDto> GetAsync(CancellationToken cancellationToken = default);
}

public class GetModerationSettingsService(IGetModerationSettingsRepository repository) : IGetModerationSettingsService
{
    public Task<GetModerationSettingsResponseDto> GetAsync(CancellationToken cancellationToken = default) =>
        repository.GetSettingsAsync(cancellationToken);
}
