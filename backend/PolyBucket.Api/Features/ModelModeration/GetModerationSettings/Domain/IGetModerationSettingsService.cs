using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;

public interface IGetModerationSettingsService
{
    Task<ModerationSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}
