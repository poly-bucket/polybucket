using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.UpdateModelVersion;

public class UpdateModelVersionControllerTests
{
    private readonly Mock<IUpdateModelVersionService> _service = new();

    [Fact(DisplayName = "When the service updates a version, the controller responds 200 OK.")]
    public async Task UpdateModelVersion_Success_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var response = new UpdateModelVersionResponse
        {
            ModelVersion = new ModelVersionDto { Id = versionId, ModelId = modelId, Name = "v2" }
        };
        _service
            .Setup(s => s.UpdateModelVersionAsync(modelId, versionId, It.IsAny<UpdateModelVersionRequest>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new UpdateModelVersionController(_service.Object, NullLogger<UpdateModelVersionController>.Instance)
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
        var result = await controller.UpdateModelVersion(
            modelId,
            versionId,
            new UpdateModelVersionRequest { Name = "v2" },
            CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(response);
    }
}