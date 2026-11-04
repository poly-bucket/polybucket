using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain;
using PolyBucket.Api.Features.Categories.DeleteAllCategories.Http;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Categories.DeleteAllCategories;

public class DeleteAllCategoriesControllerTests
{
    [Fact(DisplayName = "When deleting all categories and the service succeeds, the controller returns Ok with the deleted count.")]
    public async Task DeleteAllCategories_WhenServiceSucceeds_ReturnsOk()
    {
        var mock = new Mock<IDeleteAllCategoriesService>();
        mock
            .Setup(s => s.DeleteAllCategoriesAsync(
                It.IsAny<DeleteAllCategoriesRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAllCategoriesResponse
            {
                Success = true,
                Message = "ok",
                DeletedCount = 2,
                DeletedAt = DateTime.UtcNow
            });

        var log = new Mock<ILogger<DeleteAllCategoriesController>>();
        var controller = new DeleteAllCategoriesController(mock.Object, log.Object);
        var userId = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) },
                    "Test"))
            }
        };

        var result = await controller.DeleteAllCategories(
            new DeleteAllCategoriesRequest { AdminPassword = "s" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<DeleteAllCategoriesResponse>(ok.Value);
        Assert.Equal(2, body.DeletedCount);
    }

    [Fact(DisplayName = "When the service throws UnauthorizedAccessException, the controller returns Unauthorized.")]
    public async Task DeleteAllCategories_WhenUnauthorized_ReturnsUnauthorized()
    {
        var mock = new Mock<IDeleteAllCategoriesService>();
        mock
            .Setup(s => s.DeleteAllCategoriesAsync(
                It.IsAny<DeleteAllCategoriesRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("bad password"));

        var log = new Mock<ILogger<DeleteAllCategoriesController>>();
        var controller = new DeleteAllCategoriesController(mock.Object, log.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
                    "Test"))
            }
        };

        var result = await controller.DeleteAllCategories(
            new DeleteAllCategoriesRequest { AdminPassword = "wrong" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
