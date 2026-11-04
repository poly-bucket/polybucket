using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain
{
    public interface IDeleteAllCollectionsService
    {
        Task<DeleteAllCollectionsResponse> DeleteAllCollectionsAsync(
            DeleteAllCollectionsRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken);
    }
}
