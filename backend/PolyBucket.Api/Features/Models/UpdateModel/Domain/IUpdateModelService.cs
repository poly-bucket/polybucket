using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Models.UpdateModel.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModel.Domain
{
    public interface IUpdateModelService
    {
        Task<UpdateModelResponse> UpdateModelAsync(Guid modelId, UpdateModelRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 