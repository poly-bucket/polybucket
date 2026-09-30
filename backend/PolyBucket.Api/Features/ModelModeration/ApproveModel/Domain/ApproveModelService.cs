using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;

public class ApproveModelService(
    IApproveModelRepository repository,
    IModerationAuditLogWriter auditLogWriter,
    INotificationPublisher notificationPublisher) : IApproveModelService
{
    private readonly IApproveModelRepository _repository = repository;
    private readonly IModerationAuditLogWriter _auditLogWriter = auditLogWriter;
    private readonly INotificationPublisher _notificationPublisher = notificationPublisher;

    public async Task ApproveAsync(
        Guid modelId,
        Guid moderatorId,
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

        var previousIsPublic = model.IsPublic;
        record.Status = ModelModerationStatus.Approved;
        record.ReviewedAt = DateTime.UtcNow;
        record.ReviewedByUserId = moderatorId;
        model.IsPublic = model.Privacy != PrivacySettings.Private;
        model.UpdatedAt = DateTime.UtcNow;

        await _notificationPublisher.PublishAsync(new NotificationRequest
        {
            RecipientUserId = model.AuthorId,
            ActorUserId = moderatorId,
            Type = NotificationType.ModelApproved,
            Title = "Your model was approved",
            Message = $"\"{model.Name}\" passed moderation and is now visible to others.",
            ActionUrl = $"/models/{model.Id}",
            RelatedEntityId = model.Id,
            RelatedEntityType = "Model"
        }, cancellationToken);

        await _repository.SaveAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(
            modelId,
            moderatorId,
            ModerationAction.Approve,
            JsonSerializer.Serialize(new { IsPublic = previousIsPublic, Status = ModelModerationStatus.Pending }),
            JsonSerializer.Serialize(new { IsPublic = model.IsPublic, Status = ModelModerationStatus.Approved }),
            null,
            ipAddress,
            userAgent,
            cancellationToken);
    }
}
