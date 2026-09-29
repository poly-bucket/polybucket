using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Domain;

public class GetModelsAwaitingModerationService(IGetModelsAwaitingModerationRepository repository) : IGetModelsAwaitingModerationService
{
    private readonly IGetModelsAwaitingModerationRepository _repository = repository;

    public Task<ModelsAwaitingModerationResponse> GetAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _repository.GetPendingAsync(page, pageSize, cancellationToken);
    }
}
