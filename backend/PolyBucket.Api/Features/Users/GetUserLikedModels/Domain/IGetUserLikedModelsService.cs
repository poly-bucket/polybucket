using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

public interface IGetUserLikedModelsService
{
    Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default);
}
