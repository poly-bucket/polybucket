using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;

public interface IGetUserLikedModelsRepository
{
    Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default);
}
