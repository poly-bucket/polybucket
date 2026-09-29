using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Domain;

public interface IUpdateModerationSettingsService
{
    Task<ModerationSettingsDto> UpdateAsync(ModerationSettingsDto settings, CancellationToken cancellationToken = default);
}
