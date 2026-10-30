using PolyBucket.Api.Features.Federation.GetFederatedInstance.Http;
using PolyBucket.Api.Features.Federation.GetFederatedInstance.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstance.Domain;

public class GetFederatedInstanceService(IGetFederatedInstanceRepository repository) : IGetFederatedInstanceService
{
    public async Task<FederatedInstanceDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instance = await repository.GetAsync(id, cancellationToken);
        if (instance == null)
        {
            return null;
        }

        return new FederatedInstanceDetailDto
        {
            Id = instance.Id,
            Name = instance.Name,
            BaseUrl = instance.BaseUrl,
            Status = instance.Status.ToString(),
            Description = instance.Description,
            IsEnabled = instance.IsEnabled,
            LastSyncAt = instance.LastSyncAt,
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt
        };
    }
}
