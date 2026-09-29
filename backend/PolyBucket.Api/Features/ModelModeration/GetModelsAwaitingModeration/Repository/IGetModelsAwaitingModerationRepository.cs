using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Repository;

public interface IGetModelsAwaitingModerationRepository
{
    Task<ModelsAwaitingModerationResponse> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
