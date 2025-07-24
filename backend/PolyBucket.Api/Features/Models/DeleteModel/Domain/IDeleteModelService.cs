using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModel.Domain
{
    public interface IDeleteModelService
    {
        Task DeleteModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 