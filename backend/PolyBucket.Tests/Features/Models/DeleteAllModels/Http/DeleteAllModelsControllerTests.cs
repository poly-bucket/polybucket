using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.DeleteAllModels.Domain;
using PolyBucket.Api.Features.Models.DeleteAllModels.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteAllModels.Http;

public class DeleteAllModelsControllerTests
{
    [Fact]
    public async Task DeleteAllModels_WhenServiceSucceeds_ReturnsOk()
    {
        var mock = new Mock<IDeleteAllModelsService>();
        mock
            .Setup(s => s.DeleteAllModelsAsync(It.IsAny<DeleteAllModelsRequest>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new DeleteAllModelsResponse
                {
                    Success = true,
                    Message = "ok",
                    DeletedCount = 1,
                    DeletedAt = DateTime.UtcNow
                });

        var log = new Mock<ILogger<DeleteAllModelsController>>();
        var controller = new DeleteAllModelsController(mock.Object, log.Object);
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };

        var result = await controller.DeleteAllModels(
            new DeleteAllModelsRequest { AdminPassword = "s" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<DeleteAllModelsResponse>(ok.Value);
        Assert.Equal(1, body.DeletedCount);
    }
}
