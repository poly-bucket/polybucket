using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration.RejectModel.Http;

public class RejectModelControllerTests
{
    private readonly Mock<IRejectModelService> _service = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    private RejectModelController CreateController(Guid? userId) =>
        new RejectModelController(_service.Object).WithUser(userId);

    [Fact(DisplayName = "When rejection succeeds, the controller returns NoContent.")]
    public async Task RejectModel_Valid_ReturnsNoContent()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service
            .Setup(s => s.RejectAsync(modelId, _moderatorId, "policy", It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateController(_moderatorId).RejectModel(modelId, new RejectModelRequestDto { Reason = "policy" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When the model is missing, the controller returns NotFound.")]
    public async Task RejectModel_Missing_ReturnsNotFound()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service
            .Setup(s => s.RejectAsync(modelId, _moderatorId, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Model"));

        // Act
        var result = await CreateController(_moderatorId).RejectModel(modelId, null, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the model is not pending moderation, the controller returns Conflict.")]
    public async Task RejectModel_NotPending_ReturnsConflict()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service
            .Setup(s => s.RejectAsync(modelId, _moderatorId, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("pending"));

        // Act
        var result = await CreateController(_moderatorId).RejectModel(modelId, null, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ConflictObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task RejectModel_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.RejectModel(Guid.NewGuid(), null, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
        _service.VerifyNoOtherCalls();
    }
}
