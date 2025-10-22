using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Domain
{
    public interface IDeleteAllModelsService
    {
        Task<DeleteAllModelsResponse> DeleteAllModelsAsync(DeleteAllModelsRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
}
