using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Domain;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.DeleteModelVersion;

public class DeleteModelVersionControllerTests
{
    private readonly Mock<IDeleteModelVersionService> _service = new();

    [Fact(DisplayName = "When the service deletes a version, the controller responds 204 No Content.")]
    public async Task DeleteModelVersion_Success_ReturnsNoContent()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        _service
            .Setup(s => s.DeleteModelVersionAsync(modelId, versionId, It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = new DeleteModelVersionController(_service.Object, NullLogger<DeleteModelVersionController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
                        "Test"))
                }
            }
        };

        // Act
        var result = await controller.DeleteModelVersion(modelId, versionId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }
}