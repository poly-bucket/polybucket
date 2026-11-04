using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain;
using PolyBucket.Api.Features.Collections.DeleteAllCollections.Http;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Collections.DeleteAllCollections;

public class DeleteAllCollectionsControllerTests
{
    [Fact(DisplayName = "When deleting all collections and the service succeeds, the controller returns Ok with the deleted count.")]
    public async Task DeleteAllCollections_WhenServiceSucceeds_ReturnsOk()
    {
        var mock = new Mock<IDeleteAllCollectionsService>();
        mock
            .Setup(s => s.DeleteAllCollectionsAsync(
                It.IsAny<DeleteAllCollectionsRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAllCollectionsResponse
            {
                Success = true,
                Message = "ok",
                DeletedCount = 4,
                DeletedAt = DateTime.UtcNow
            });

        var log = new Mock<ILogger<DeleteAllCollectionsController>>();
        var controller = new DeleteAllCollectionsController(mock.Object, log.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim> { new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) },
                    "Test"))
            }
        };

        var result = await controller.DeleteAllCollections(
            new DeleteAllCollectionsRequest { AdminPassword = "s" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<DeleteAllCollectionsResponse>(ok.Value);
        Assert.Equal(4, body.DeletedCount);
    }

    [Fact(DisplayName = "When the service throws UnauthorizedAccessException, the controller returns Unauthorized.")]
    public async Task DeleteAllCollections_WhenUnauthorized_ReturnsUnauthorized()
    {
        var mock = new Mock<IDeleteAllCollectionsService>();
        mock
            .Setup(s => s.DeleteAllCollectionsAsync(
                It.IsAny<DeleteAllCollectionsRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("bad password"));

        var log = new Mock<ILogger<DeleteAllCollectionsController>>();
        var controller = new DeleteAllCollectionsController(mock.Object, log.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
                    "Test"))
            }
        };

        var result = await controller.DeleteAllCollections(
            new DeleteAllCollectionsRequest { AdminPassword = "wrong" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
