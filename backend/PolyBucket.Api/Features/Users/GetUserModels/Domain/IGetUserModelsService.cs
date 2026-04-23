using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserModels.Domain;

public interface IGetUserModelsService
{
    Task<GetUserModelsResult> GetUserPublicModelsAsync(GetUserModelsQuery query, CancellationToken cancellationToken = default);
}
