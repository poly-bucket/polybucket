using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Repository;

public interface IGetPublicUserCollectionsRepository
{
    Task<GetPublicUserCollectionsResult> GetPublicUserCollectionsAsync(GetPublicUserCollectionsQuery query, CancellationToken cancellationToken = default);
}
