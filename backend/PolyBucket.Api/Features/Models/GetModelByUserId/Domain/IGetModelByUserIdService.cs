using PolyBucket.Api.Features.Models.GetModelByUserId.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Domain
{
    public interface IGetModelByUserIdService
    {
        Task<GetModelByUserIdResponse> GetModelsByUserIdAsync(Guid userId, GetModelByUserIdRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 