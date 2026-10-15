using PolyBucket.Api.Features.Collections.GetUserCollections.Domain;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Repository
{
    public interface ICollectionRepository
    {
        Task<GetUserCollectionsResponseDto> GetUserCollectionsAsync(GetUserCollectionsQuery query, CancellationToken cancellationToken = default);
    }
}
