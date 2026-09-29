using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Repository;

public interface IUpdateModerationSettingsRepository
{
    Task UpdateAsync(ModerationSettingsDto settings, CancellationToken cancellationToken = default);
}
