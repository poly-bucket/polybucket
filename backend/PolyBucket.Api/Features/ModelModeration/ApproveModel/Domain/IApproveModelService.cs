using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;

public interface IApproveModelService
{
    Task ApproveAsync(
        Guid modelId,
        Guid moderatorId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
