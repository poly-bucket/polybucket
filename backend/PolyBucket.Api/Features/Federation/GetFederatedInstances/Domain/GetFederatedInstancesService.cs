using PolyBucket.Api.Features.Federation.GetFederatedInstances.Http;
using PolyBucket.Api.Features.Federation.GetFederatedInstances.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstances.Domain;

public class GetFederatedInstancesService(IGetFederatedInstancesRepository repository) : IGetFederatedInstancesService
{
    public async Task<IReadOnlyList<FederatedInstanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var instances = await repository.GetAllAsync(cancellationToken);
        return instances.Select(i => new FederatedInstanceDto
        {
            Id = i.Id,
            Name = i.Name,
            BaseUrl = i.BaseUrl,
            Status = i.Status.ToString(),
            Description = i.Description,
            IsEnabled = i.IsEnabled,
            LastSyncAt = i.LastSyncAt,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        }).ToList();
    }
}
