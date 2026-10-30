using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.CanUserModerateUser.Domain;
using PolyBucket.Api.Features.ACL.CanUserModerateUser.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.CanUserModerateUser.Http;

public class CanUserModerateUserControllerTests
{
    [Fact(DisplayName = "When moderation capability is checked, CanUserModerateUser returns the service result.")]
    public async Task CanUserModerateUser_ReturnsDto()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var service = new Mock<ICanUserModerateUserService>();
        service.Setup(s => s.CanUserModerateUserAsync(moderatorId, targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModerationCheckResponseDto { CanModerate = true, ModeratorRole = "Admin", TargetRole = "User" });
        var controller = new CanUserModerateUserController(service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.CanUserModerateUser(moderatorId, targetId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<ModerationCheckResponseDto>().CanModerate.ShouldBeTrue();
    }
}
