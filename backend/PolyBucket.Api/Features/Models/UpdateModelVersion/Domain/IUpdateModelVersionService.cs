using PolyBucket.Api.Features.Models.UpdateModelVersion.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Domain
{
    public interface IUpdateModelVersionService
    {
        Task<UpdateModelVersionResponse> UpdateModelVersionAsync(Guid modelId, Guid versionId, UpdateModelVersionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 