using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Domain;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.RegenerateAvatar.Http;

public class RegenerateAvatarControllerTests
{
    private readonly Mock<IRegenerateAvatarService> _service = new();

    [Fact(DisplayName = "When regeneration succeeds, regenerate avatar returns Ok.")]
    public async Task RegenerateAvatar_Valid_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var response = new RegenerateAvatarResponse { Avatar = "/avatars/x.png", UserId = userId };
        _service.Setup(s => s.RegenerateAvatarAsync(userId, "salt", null)).ReturnsAsync(response);
        var controller = new RegenerateAvatarController(_service.Object).WithUser(userId);

        // Act
        var result = await controller.RegenerateAvatar(new RegenerateAvatarRequest { Salt = "salt" });

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the caller submits avatar SVG, it is forwarded to the service.")]
    public async Task RegenerateAvatar_WithClientSvg_ForwardsAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string svg =
            "<svg viewBox=\"-1.5 -1.5 8 8\" xmlns=\"http://www.w3.org/2000/svg\"></svg>";
        var response = new RegenerateAvatarResponse { Avatar = svg, UserId = userId, Salt = "seed" };
        _service
            .Setup(s => s.RegenerateAvatarAsync(userId, "seed", svg))
            .ReturnsAsync(response);
        var controller = new RegenerateAvatarController(_service.Object).WithUser(userId);

        // Act
        var result = await controller.RegenerateAvatar(
            new RegenerateAvatarRequest { Salt = "seed", Avatar = svg });

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _service.Verify(s => s.RegenerateAvatarAsync(userId, "seed", svg), Times.Once);
    }

    [Fact(DisplayName = "When the caller has no user id claim, regenerate avatar returns Unauthorized.")]
    public async Task RegenerateAvatar_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new RegenerateAvatarController(_service.Object).WithUser(null);

        // Act
        var result = await controller.RegenerateAvatar(new RegenerateAvatarRequest());

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }
}
