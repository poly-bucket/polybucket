using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Domain;

public class UpdateModerationSettingsService(IUpdateModerationSettingsRepository repository) : IUpdateModerationSettingsService
{
    private readonly IUpdateModerationSettingsRepository _repository = repository;

    public async Task<ModerationSettingsDto> UpdateAsync(ModerationSettingsDto settings, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(settings, cancellationToken);
        return settings;
    }
}
