using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserSettings.Domain;
using PolyBucket.Api.Features.Users.GetUserSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserSettings.Http;

public class GetUserSettingsControllerTests
{
    private readonly Mock<IGetUserSettingsService> _service = new();
    private readonly Mock<ILogger<GetUserSettingsController>> _logger = new();

    [Fact(DisplayName = "When settings exist, get user settings returns Ok.")]
    public async Task GetUserSettings_Found_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = new GetUserSettingsResult { Settings = new PolyBucket.Api.Features.Users.Domain.UserSettings() };
        _service
            .Setup(s => s.GetUserSettingsAsync(It.Is<GetUserSettingsRequest>(r => r.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        var controller = new GetUserSettingsController(_service.Object, _logger.Object).WithUser(userId);

        // Act
        var response = await controller.GetUserSettings(CancellationToken.None);

        // Assert
        response.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(result);
    }

    [Fact(DisplayName = "When the caller has no user id claim, get user settings returns Unauthorized.")]
    public async Task GetUserSettings_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new GetUserSettingsController(_service.Object, _logger.Object).WithUser(null);

        // Act
        var response = await controller.GetUserSettings(CancellationToken.None);

        // Assert
        response.Result.ShouldBeOfType<UnauthorizedResult>();
    }
}
