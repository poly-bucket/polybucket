using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.GetFollowerHistory;

public class GetFollowerHistoryControllerTests
{
    [Fact(DisplayName = "When days is invalid, the follower history controller returns BadRequest.")]
    public async Task GetFollowerHistory_InvalidDays_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IGetFollowerHistoryService>();
        service
            .Setup(s => s.GetHistoryAsync(It.IsAny<GetFollowerHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetFollowerHistoryOutcome.InvalidDays, null));

        var controller = CreateController(service.Object, userId);
        var result = await controller.GetFollowerHistory(userId, 14, CancellationToken.None);

        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When viewer is not the owner, the follower history controller returns 403.")]
    public async Task GetFollowerHistory_Forbidden_Returns403()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IGetFollowerHistoryService>();
        service
            .Setup(s => s.GetHistoryAsync(It.IsAny<GetFollowerHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetFollowerHistoryOutcome.Forbidden, null));

        var controller = CreateController(service.Object, userId);
        var result = await controller.GetFollowerHistory(userId, 30, CancellationToken.None);

        result.Result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(403);
    }

    private static GetFollowerHistoryController CreateController(IGetFollowerHistoryService service, Guid userId)
    {
        return new GetFollowerHistoryController(service)
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity(
                            [new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.NameIdentifier,
                                userId.ToString())],
                            "test"))
                }
            }
        };
    }
}
