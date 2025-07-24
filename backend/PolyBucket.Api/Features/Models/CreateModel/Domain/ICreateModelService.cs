using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Domain
{
    public interface ICreateModelService
    {
        Task<CreateModelResponse> CreateModelAsync(CreateModelRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);
    }
} 