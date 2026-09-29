using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Repository;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;

public class RejectModelService(
    IRejectModelRepository repository,
    IModerationAuditLogWriter auditLogWriter) : IRejectModelService
{
    private readonly IRejectModelRepository _repository = repository;
    private readonly IModerationAuditLogWriter _auditLogWriter = auditLogWriter;

    public async Task RejectAsync(
        Guid modelId,
        Guid moderatorId,
        string? reason,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var model = await _repository.GetModelAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new NotFoundException("Model not found");
        }

        var record = await _repository.GetModerationRecordAsync(modelId, cancellationToken);
        if (record == null || record.Status != ModelModerationStatus.Pending)
        {
            throw new ConflictException("Model is not pending moderation");
        }

        record.Status = ModelModerationStatus.Rejected;
        record.ReviewedAt = DateTime.UtcNow;
        record.ReviewedByUserId = moderatorId;
        record.RejectionReason = reason;
        model.IsPublic = false;
        model.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(
            modelId,
            moderatorId,
            ModerationAction.Reject,
            JsonSerializer.Serialize(new { Status = ModelModerationStatus.Pending }),
            JsonSerializer.Serialize(new { Status = ModelModerationStatus.Rejected, Reason = reason }),
            reason,
            ipAddress,
            userAgent,
            cancellationToken);
    }
}
