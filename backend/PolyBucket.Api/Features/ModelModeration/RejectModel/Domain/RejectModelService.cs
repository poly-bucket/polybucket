using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Repository;
using PolyBucket.Api.Features.Notifications.Domain;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;

public class RejectModelService(
    IRejectModelRepository repository,
    IModerationAuditLogWriter auditLogWriter,
    INotificationPublisher notificationPublisher) : IRejectModelService
{
    private readonly IRejectModelRepository _repository = repository;
    private readonly IModerationAuditLogWriter _auditLogWriter = auditLogWriter;
    private readonly INotificationPublisher _notificationPublisher = notificationPublisher;

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

        await _notificationPublisher.PublishAsync(new NotificationRequest
        {
            RecipientUserId = model.AuthorId,
            ActorUserId = moderatorId,
            Type = NotificationType.ModelRejected,
            Title = "Your model was not approved",
            Message = string.IsNullOrWhiteSpace(reason)
                ? $"\"{model.Name}\" did not pass moderation."
                : $"\"{model.Name}\" did not pass moderation. Reason: {reason.Trim()}",
            ActionUrl = $"/models/{model.Id}",
            RelatedEntityId = model.Id,
            RelatedEntityType = "Model",
            Priority = NotificationPriority.High
        }, cancellationToken);

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
