using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.FollowUser.Domain;
using PolyBucket.Api.Features.Users.FollowUser.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.FollowUser;

public class FollowUserControllerTests
{
    [Fact(DisplayName = "When follow succeeds, the follow user controller returns Ok.")]
    public async Task FollowUser_Success_ReturnsOk()
    {
        var followerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var service = new Mock<IFollowUserService>();
        service
            .Setup(s => s.FollowAsync(followerId, targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FollowUserOutcome.Success);

        var controller = new FollowUserController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, followerId.ToString())],
                        "test"))
                }
            }
        };

        var result = await controller.FollowUser(targetId, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When follow targets a private profile, the follow user controller returns 403.")]
    public async Task FollowUser_PrivateTarget_ReturnsForbidden()
    {
        var followerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var service = new Mock<IFollowUserService>();
        service
            .Setup(s => s.FollowAsync(followerId, targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FollowUserOutcome.TargetPrivate);

        var controller = new FollowUserController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, followerId.ToString())],
                        "test"))
                }
            }
        };

        var result = await controller.FollowUser(targetId, CancellationToken.None);

        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(403);
    }
}
