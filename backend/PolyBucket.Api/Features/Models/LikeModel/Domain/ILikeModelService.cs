using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Domain;

public interface ILikeModelService
{
    Task LikeModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken);
    Task UnlikeModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken);
}
