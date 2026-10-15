using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;

public interface IModeratorEditModelService
{
    Task<ModeratorEditModelResponseDto> EditModelAsync(
        Guid modelId,
        Guid moderatorId,
        ModeratorEditRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task<ModeratorEditModelResponseDto?> GetModelForModerationAsync(Guid modelId, CancellationToken cancellationToken = default);
}
