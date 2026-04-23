using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserModels.Repository;

namespace PolyBucket.Api.Features.Users.GetUserModels.Domain;

public class GetUserModelsService(IGetUserModelsRepository repository) : IGetUserModelsService
{
    public Task<GetUserModelsResult> GetUserPublicModelsAsync(GetUserModelsQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetUserPublicModelsAsync(query, cancellationToken);
    }
}
