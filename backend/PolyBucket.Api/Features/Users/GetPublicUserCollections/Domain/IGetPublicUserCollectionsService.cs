using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

public interface IGetPublicUserCollectionsService
{
    Task<GetPublicUserCollectionsResult> GetPublicUserCollectionsAsync(GetPublicUserCollectionsQuery query, CancellationToken cancellationToken = default);
}
