using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.GetModelVersions.Domain;
using PolyBucket.Api.Features.Models.GetModelVersions.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelVersions;

public class GetModelVersionsControllerTests
{
    private readonly Mock<IGetModelVersionsService> _service = new();

    private GetModelVersionsController CreateController(Guid? userId)
    {
        var claims = userId.HasValue ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) } : Array.Empty<Claim>();
        return new GetModelVersionsController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, userId.HasValue ? "Test" : null))
                }
            }
        };
    }

    [Fact(DisplayName = "When the service returns versions, the controller responds 200 with them and passes the signed-in user.")]
    public async Task GetModelVersions_Visible_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var response = new GetModelVersionsResponse { ModelId = modelId, Versions = new List<ModelVersionDto> { new() { Name = "v1" } } };
        _service.Setup(s => s.GetModelVersionsAsync(modelId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = CreateController(userId);

        // Act
        var result = await controller.GetModelVersions(modelId, CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the model is missing or hidden, the controller responds 404.")]
    public async Task GetModelVersions_NotVisible_ReturnsNotFound()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.GetModelVersionsAsync(modelId, null, It.IsAny<CancellationToken>())).ReturnsAsync((GetModelVersionsResponse?)null);
        var controller = CreateController(null);

        // Act
        var result = await controller.GetModelVersions(modelId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "When the caller is anonymous, the service is asked with no viewer.")]
    public async Task GetModelVersions_Anonymous_PassesNullViewer()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var controller = CreateController(null);

        // Act
        await controller.GetModelVersions(modelId, CancellationToken.None);

        // Assert
        _service.Verify(s => s.GetModelVersionsAsync(modelId, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
