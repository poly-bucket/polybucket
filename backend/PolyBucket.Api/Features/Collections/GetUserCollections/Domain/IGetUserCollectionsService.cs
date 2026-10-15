using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Domain;

public interface IGetUserCollectionsService
{
    Task<GetUserCollectionsResponseDto> GetUserCollectionsAsync(GetUserCollectionsQuery query, CancellationToken cancellationToken = default);
}
