using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UpdateUserSettings.Http;

public class UpdateUserSettingsControllerTests
{
    private readonly Mock<IUpdateUserSettingsService> _service = new();
    private readonly Mock<ILogger<UpdateUserSettingsController>> _logger = new();

    [Fact(DisplayName = "When update succeeds, update user settings returns Ok.")]
    public async Task UpdateUserSettings_Valid_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserSettingsCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new UpdateUserSettingsController(_service.Object, _logger.Object).WithUser(userId);
        var request = new UpdateUserSettingsRequest { Language = "en", Theme = "dark" };

        // Act
        var result = await controller.UpdateUserSettings(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, update user settings returns Unauthorized.")]
    public async Task UpdateUserSettings_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new UpdateUserSettingsController(_service.Object, _logger.Object).WithUser(null);

        // Act
        var result = await controller.UpdateUserSettings(new UpdateUserSettingsRequest(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }
}
