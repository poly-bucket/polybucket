using PolyBucket.Api.Features.Federation.GetFederatedInstance.Http;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstance.Domain;

public interface IGetFederatedInstanceService
{
    Task<FederatedInstanceDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
