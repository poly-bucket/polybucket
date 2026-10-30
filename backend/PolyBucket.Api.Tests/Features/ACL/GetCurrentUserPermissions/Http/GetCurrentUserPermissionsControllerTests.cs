using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Domain;
using PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.GetCurrentUserPermissions.Http;

public class GetCurrentUserPermissionsControllerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the current user has a role, GetCurrentUserPermissions returns Ok.")]
    public async Task GetCurrentUserPermissions_ValidUser_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetCurrentUserPermissionsService>();
        service.Setup(s => s.GetCurrentUserPermissionsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserPermissionsResponseDto
            {
                UserId = _userId,
                Role = new UserRoleSummaryDto { Id = Guid.NewGuid(), Name = "User", Priority = 100 },
                EffectivePermissions = new List<string> { "model.view" }
            });
        var controller = new GetCurrentUserPermissionsController(service.Object).WithUser(_userId);

        // Act
        var result = await controller.GetCurrentUserPermissions(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<UserPermissionsResponseDto>().UserId.ShouldBe(_userId);
    }

    [Fact(DisplayName = "When the current user has no user id claim, GetCurrentUserPermissions returns Unauthorized.")]
    public async Task GetCurrentUserPermissions_NoClaim_ReturnsUnauthorized()
    {
        // Arrange
        var service = new Mock<IGetCurrentUserPermissionsService>();
        var controller = new GetCurrentUserPermissionsController(service.Object).WithUser(null);

        // Act
        var result = await controller.GetCurrentUserPermissions(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedResult>();
        service.VerifyNoOtherCalls();
    }
}
