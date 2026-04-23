using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserModels.Repository;

public interface IGetUserModelsRepository
{
    Task<GetUserModelsResult> GetUserPublicModelsAsync(GetUserModelsQuery query, CancellationToken cancellationToken = default);
}
