using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateModelPreview;

public class GenerateModelPreviewControllerTests
{
    private readonly Mock<IGenerateModelPreviewService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _modelId = Guid.NewGuid();

    private GenerateModelPreviewController CreateController(bool authenticated = true)
    {
        var claims = authenticated ? new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) } : Array.Empty<Claim>();
        return new GenerateModelPreviewController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null)) }
            }
        };
    }

    [Fact(DisplayName = "When the preview is queued, the controller responds 200 with the queue status.")]
    public async Task Queued_ReturnsOk()
    {
        // Arrange
        var response = new GenerateModelPreviewResponse { ModelId = _modelId, Size = "thumbnail", IsQueued = true };
        _service.Setup(s => s.RequestPreviewAsync(_modelId, "thumbnail", true, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        // Act
        var result = await CreateController().GenerateModelPreview(_modelId, "thumbnail", true, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the size is invalid, the controller responds 400.")]
    public async Task InvalidSize_ReturnsBadRequest()
    {
        // Arrange
        _service.Setup(s => s.RequestPreviewAsync(_modelId, "huge", false, _userId, It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("bad size"));

        // Act
        var result = await CreateController().GenerateModelPreview(_modelId, "huge", false, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the model does not exist, the controller responds 404.")]
    public async Task MissingModel_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.RequestPreviewAsync(_modelId, "thumbnail", false, _userId, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await CreateController().GenerateModelPreview(_modelId, "thumbnail", false, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "When the caller does not own the model, the controller responds 403.")]
    public async Task NotOwner_ReturnsForbid()
    {
        // Arrange
        _service.Setup(s => s.RequestPreviewAsync(_modelId, "thumbnail", false, _userId, It.IsAny<CancellationToken>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await CreateController().GenerateModelPreview(_modelId, "thumbnail", false, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller responds 401 without calling the service.")]
    public async Task NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(authenticated: false);

        // Act
        var result = await controller.GenerateModelPreview(_modelId, "thumbnail", false, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedResult>();
        _service.VerifyNoOtherCalls();
    }
}
