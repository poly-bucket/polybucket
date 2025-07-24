using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModelVersion.Domain
{
    public interface IDeleteModelVersionService
    {
        Task DeleteModelVersionAsync(Guid modelId, Guid versionId, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 