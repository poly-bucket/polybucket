using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

public class GetUserLikedModelsService(IGetUserLikedModelsRepository repository) : IGetUserLikedModelsService
{
    public Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetUserLikedModelsAsync(query, cancellationToken);
    }
}
