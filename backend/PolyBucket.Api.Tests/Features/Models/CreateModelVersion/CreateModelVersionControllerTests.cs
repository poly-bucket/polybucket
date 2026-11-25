using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.CreateModelVersion;

public class CreateModelVersionControllerTests
{
    private readonly Mock<ICreateModelVersionService> _service = new();

    [Fact(DisplayName = "When the service returns a version, the controller responds 201 Created.")]
    public async Task CreateModelVersion_Success_ReturnsCreated()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var response = new CreateModelVersionResponse
        {
            ModelVersion = new ModelVersionDto { Id = versionId, ModelId = modelId, Name = "v2" }
        };
        _service
            .Setup(s => s.CreateModelVersionAsync(modelId, It.IsAny<CreateModelVersionRequest>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new CreateModelVersionController(_service.Object, NullLogger<CreateModelVersionController>.Instance)
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
        var result = await controller.CreateModelVersion(
            modelId,
            "v2",
            null,
            null,
            [],
            CancellationToken.None);

        // Assert
        var created = result.Result.ShouldBeOfType<CreatedResult>();
        created.Value.ShouldBe(response);
    }
}