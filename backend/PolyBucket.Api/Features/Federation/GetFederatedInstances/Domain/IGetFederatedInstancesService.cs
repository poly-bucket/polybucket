using PolyBucket.Api.Features.Federation.GetFederatedInstances.Http;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstances.Domain;

public interface IGetFederatedInstancesService
{
    Task<IReadOnlyList<FederatedInstanceDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
