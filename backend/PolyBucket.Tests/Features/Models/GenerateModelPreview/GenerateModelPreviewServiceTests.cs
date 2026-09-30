using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateModelPreview;

public class GenerateModelPreviewServiceTests
{
    private static readonly Guid ModelId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();

    private readonly Mock<IModelPreviewQueueRepository> _repository = new();
    private readonly Mock<IModelPreviewQueue> _queue = new();
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly GenerateModelPreviewService _service;

    public GenerateModelPreviewServiceTests()
    {
        _repository.Setup(r => r.GetModelAuthorIdAsync(ModelId, It.IsAny<CancellationToken>())).ReturnsAsync(OwnerId);
        _queue
            .Setup(q => q.EnqueueAsync(ModelId, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, string size, bool _, CancellationToken _) => new ModelPreview { ModelId = id, Size = size, Status = PreviewStatus.Pending });
        _service = new GenerateModelPreviewService(_repository.Object, _queue.Object, _permissions.Object, TimeProvider.System);
    }

    [Fact(DisplayName = "When the owner requests a preview, it is queued with the normalized size.")]
    public async Task Owner_QueuesPreview()
    {
        // Arrange
        var size = " Medium ";

        // Act
        var response = await _service.RequestPreviewAsync(ModelId, size, forceRegenerate: true, OwnerId, CancellationToken.None);

        // Assert
        response.IsQueued.ShouldBeTrue();
        response.Size.ShouldBe("medium");
        response.Status.ShouldBe(PreviewStatus.Pending);
        _queue.Verify(q => q.EnqueueAsync(ModelId, "medium", true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the preview is already completed, the response says so and is not marked queued.")]
    public async Task Completed_NotQueued()
    {
        // Arrange
        _queue
            .Setup(q => q.EnqueueAsync(ModelId, "thumbnail", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelPreview { Status = PreviewStatus.Completed });

        // Act
        var response = await _service.RequestPreviewAsync(ModelId, "thumbnail", forceRegenerate: false, OwnerId, CancellationToken.None);

        // Assert
        response.IsQueued.ShouldBeFalse();
        response.Message.ShouldBe("Preview already exists");
    }

    [Fact(DisplayName = "When the size is not supported, the request is rejected before touching the queue.")]
    public async Task InvalidSize_Throws()
    {
        // Arrange
        var size = "gigantic";

        // Act
        var act = () => _service.RequestPreviewAsync(ModelId, size, false, OwnerId, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        _queue.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the model does not exist, a not-found error is raised.")]
    public async Task MissingModel_Throws()
    {
        // Arrange
        _repository.Setup(r => r.GetModelAuthorIdAsync(ModelId, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        // Act
        var act = () => _service.RequestPreviewAsync(ModelId, "thumbnail", false, OwnerId, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<KeyNotFoundException>(act);
    }

    [Fact(DisplayName = "When another user without edit-any permission requests a preview, access is denied.")]
    public async Task OtherUser_WithoutPermission_Throws()
    {
        // Arrange
        var otherUser = Guid.NewGuid();
        _permissions.Setup(p => p.HasPermissionAsync(otherUser, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);

        // Act
        var act = () => _service.RequestPreviewAsync(ModelId, "thumbnail", true, otherUser, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(act);
        _queue.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a user with edit-any permission requests a preview for someone else's model, it is queued.")]
    public async Task OtherUser_WithPermission_Queues()
    {
        // Arrange
        var admin = Guid.NewGuid();
        _permissions.Setup(p => p.HasPermissionAsync(admin, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(true);

        // Act
        var response = await _service.RequestPreviewAsync(ModelId, "thumbnail", true, admin, CancellationToken.None);

        // Assert
        response.IsQueued.ShouldBeTrue();
    }
}
