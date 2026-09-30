using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration;

public class ApproveModelServiceTests
{
    [Fact]
    public async Task ApproveAsync_WhenModelPending_SetsApprovedAndPublic()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var model = new Model
        {
            Id = modelId,
            Privacy = PrivacySettings.Public,
            IsPublic = false
        };
        var record = new ModelModerationRecord
        {
            ModelId = modelId,
            Status = ModelModerationStatus.Pending
        };

        var repository = new Mock<IApproveModelRepository>();
        repository.Setup(r => r.GetModelAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);
        repository.Setup(r => r.GetModerationRecordAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        repository.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var audit = new Mock<IModerationAuditLogWriter>();
        audit.Setup(a => a.WriteAsync(
            modelId,
            moderatorId,
            ModerationAction.Approve,
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            null,
            null,
            null,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new ApproveModelService(repository.Object, audit.Object, Mock.Of<INotificationPublisher>());

        // Act
        await service.ApproveAsync(modelId, moderatorId, null, null);

        // Assert
        record.Status.ShouldBe(ModelModerationStatus.Approved);
        model.IsPublic.ShouldBeTrue();
        repository.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_WhenModelPending_NotifiesAuthorBeforeSaving()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var model = new Model { Id = modelId, Name = "Benchy", AuthorId = authorId, Privacy = PrivacySettings.Public };
        var record = new ModelModerationRecord { ModelId = modelId, Status = ModelModerationStatus.Pending };
        var calls = new System.Collections.Generic.List<string>();

        var repository = new Mock<IApproveModelRepository>();
        repository.Setup(r => r.GetModelAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);
        repository.Setup(r => r.GetModerationRecordAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        repository.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Callback(() => calls.Add("save")).Returns(Task.CompletedTask);

        NotificationRequest? published = null;
        var publisher = new Mock<INotificationPublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationRequest, CancellationToken>((r, _) => { published = r; calls.Add("publish"); })
            .ReturnsAsync(true);

        var service = new ApproveModelService(repository.Object, Mock.Of<IModerationAuditLogWriter>(), publisher.Object);

        // Act
        await service.ApproveAsync(modelId, moderatorId, null, null);

        // Assert
        published.ShouldNotBeNull();
        published.RecipientUserId.ShouldBe(authorId);
        published.ActorUserId.ShouldBe(moderatorId);
        published.Type.ShouldBe(NotificationType.ModelApproved);
        published.ActionUrl.ShouldBe($"/models/{modelId}");
        published.Message.ShouldContain("Benchy");
        calls.ShouldBe(new[] { "publish", "save" });
    }

    [Fact]
    public async Task ApproveAsync_WhenNotPending_ThrowsConflict()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var repository = new Mock<IApproveModelRepository>();
        repository.Setup(r => r.GetModelAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId });
        repository.Setup(r => r.GetModerationRecordAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelModerationRecord { Status = ModelModerationStatus.Approved });
        var publisher = new Mock<INotificationPublisher>();

        var service = new ApproveModelService(repository.Object, Mock.Of<IModerationAuditLogWriter>(), publisher.Object);

        // Act
        var act = () => service.ApproveAsync(modelId, Guid.NewGuid(), null, null);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
        publisher.Verify(p => p.PublishAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
