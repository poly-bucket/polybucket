using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Http;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.DeleteAllFileSettings;

public class DeleteAllFileSettingsControllerTests
{
    [Fact(DisplayName = "When deleting all file settings and the service succeeds, the controller returns Ok with the deleted count.")]
    public async Task DeleteAllFileSettings_WhenServiceSucceeds_ReturnsOk()
    {
        var mock = new Mock<IDeleteAllFileSettingsService>();
        mock
            .Setup(s => s.DeleteAllFileSettingsAsync(
                It.IsAny<DeleteAllFileSettingsRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAllFileSettingsResponse
            {
                Success = true,
                Message = "ok",
                DeletedCount = 3,
                DeletedAt = DateTime.UtcNow
            });

        var log = new Mock<ILogger<DeleteAllFileSettingsController>>();
        var controller = new DeleteAllFileSettingsController(mock.Object, log.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new(ClaimTypes.Role, "Admin")
                    },
                    "Test"))
            }
        };

        var result = await controller.DeleteAllFileSettings(
            new DeleteAllFileSettingsRequest { AdminPassword = "s" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<DeleteAllFileSettingsResponse>(ok.Value);
        Assert.Equal(3, body.DeletedCount);
    }

    [Fact(DisplayName = "When the service throws UnauthorizedAccessException, the controller returns Unauthorized.")]
    public async Task DeleteAllFileSettings_WhenUnauthorized_ReturnsUnauthorized()
    {
        var mock = new Mock<IDeleteAllFileSettingsService>();
        mock
            .Setup(s => s.DeleteAllFileSettingsAsync(
                It.IsAny<DeleteAllFileSettingsRequest>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("bad password"));

        var log = new Mock<ILogger<DeleteAllFileSettingsController>>();
        var controller = new DeleteAllFileSettingsController(mock.Object, log.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
                    "Test"))
            }
        };

        var result = await controller.DeleteAllFileSettings(
            new DeleteAllFileSettingsRequest { AdminPassword = "wrong" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
