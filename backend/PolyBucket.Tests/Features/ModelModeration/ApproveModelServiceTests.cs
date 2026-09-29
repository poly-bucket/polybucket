using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Domain;
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

        var service = new ApproveModelService(repository.Object, audit.Object);

        // Act
        await service.ApproveAsync(modelId, moderatorId, null, null);

        // Assert
        record.Status.ShouldBe(ModelModerationStatus.Approved);
        model.IsPublic.ShouldBeTrue();
        repository.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        var service = new ApproveModelService(repository.Object, Mock.Of<IModerationAuditLogWriter>());

        // Act
        var act = () => service.ApproveAsync(modelId, Guid.NewGuid(), null, null);

        // Assert
        await act.ShouldThrowAsync<ConflictException>();
    }
}
