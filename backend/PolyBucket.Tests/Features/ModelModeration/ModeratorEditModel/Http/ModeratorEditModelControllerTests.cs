using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration.ModeratorEditModel.Http;

public class ModeratorEditModelControllerTests
{
    private readonly Mock<IModeratorEditModelService> _service = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    private ModeratorEditModelController CreateController(Guid? userId) =>
        new ModeratorEditModelController(_service.Object).WithUser(userId);

    private static ModeratorEditRequest EditRequest() => new()
    {
        Name = "Updated name",
        Description = "Updated description with enough length",
        Action = ModerationAction.Edit
    };

    [Fact(DisplayName = "When edit succeeds, the controller returns Ok with the model.")]
    public async Task EditModel_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var model = new Model { Id = modelId, Name = "Updated name" };
        _service
            .Setup(s => s.EditModelAsync(modelId, _moderatorId, It.IsAny<ModeratorEditRequest>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // Act
        var result = await CreateController(_moderatorId).EditModel(modelId, EditRequest(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(model);
    }

    [Fact(DisplayName = "When the model is missing on edit, the controller returns NotFound.")]
    public async Task EditModel_Missing_ReturnsNotFound()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service
            .Setup(s => s.EditModelAsync(modelId, _moderatorId, It.IsAny<ModeratorEditRequest>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Model"));

        // Act
        var result = await CreateController(_moderatorId).EditModel(modelId, EditRequest(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim on edit, the controller returns Unauthorized.")]
    public async Task EditModel_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.EditModel(Guid.NewGuid(), EditRequest(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "Get model for moderation returns Ok when the model exists.")]
    public async Task GetModelForModeration_Found_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var model = new Model { Id = modelId, Name = "Pending" };
        _service.Setup(s => s.GetModelForModerationAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        // Act
        var result = await CreateController(_moderatorId).GetModelForModeration(modelId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(model);
    }

    [Fact(DisplayName = "Get model for moderation returns NotFound when the model is missing.")]
    public async Task GetModelForModeration_Missing_ReturnsNotFound()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.GetModelForModerationAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync((Model?)null);

        // Act
        var result = await CreateController(_moderatorId).GetModelForModeration(modelId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
