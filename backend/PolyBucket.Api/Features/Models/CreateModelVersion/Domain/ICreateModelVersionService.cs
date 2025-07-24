using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Domain
{
    public interface ICreateModelVersionService
    {
        Task<CreateModelVersionResponse> CreateModelVersionAsync(Guid modelId, CreateModelVersionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 