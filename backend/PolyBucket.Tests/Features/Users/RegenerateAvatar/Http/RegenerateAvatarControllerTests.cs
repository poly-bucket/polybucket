using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Domain;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.RegenerateAvatar.Http;

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
