using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain
{
    public interface IDeleteAllCategoriesService
    {
        Task<DeleteAllCategoriesResponse> DeleteAllCategoriesAsync(
            DeleteAllCategoriesRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken);
    }
}
