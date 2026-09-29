using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;

public interface IRejectModelService
{
    Task RejectAsync(
        Guid modelId,
        Guid moderatorId,
        string? reason,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
