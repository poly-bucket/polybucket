using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Domain;

public interface IGetModelsAwaitingModerationService
{
    Task<ModelsAwaitingModerationResponse> GetAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
