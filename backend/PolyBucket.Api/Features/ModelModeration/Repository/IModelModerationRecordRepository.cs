using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Repository;

public interface IModelModerationRecordRepository
{
    Task<ModelModerationRecord?> GetByModelIdAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task<bool> IsAuthorEmailVerifiedAsync(Guid authorId, CancellationToken cancellationToken = default);

    Task UpsertRecordAsync(ModelModerationRecord record, CancellationToken cancellationToken = default);

    Task UpdateModelAsync(Model model, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
