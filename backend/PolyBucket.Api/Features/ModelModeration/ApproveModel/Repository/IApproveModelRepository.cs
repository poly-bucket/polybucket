using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;

public interface IApproveModelRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task<ModelModerationRecord?> GetModerationRecordAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task SaveAsync(CancellationToken cancellationToken = default);
}
