using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;

public interface IModeratorEditModelService
{
    Task<Model> EditModelAsync(
        Guid modelId,
        Guid moderatorId,
        ModeratorEditRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task<Model?> GetModelForModerationAsync(Guid modelId, CancellationToken cancellationToken = default);
}
