using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetRole.Domain;
using PolyBucket.Api.Features.ACL.GetRole.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.GetRole.Http;

public class GetRoleControllerTests
{
    [Fact(DisplayName = "When a role exists, GetRole returns Ok with the role DTO.")]
    public async Task GetRole_Found_ReturnsOk()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var dto = new RoleDto { Id = roleId, Name = "Moderator", UserCount = 3 };
        var service = new Mock<IGetRoleService>();
        service.Setup(s => s.GetRoleAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var controller = new GetRoleController(service.Object);

        // Act
        var result = await controller.GetRole(roleId, CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<RoleDto>().Id.ShouldBe(roleId);
    }

    [Fact(DisplayName = "When a role does not exist, GetRole returns NotFound.")]
    public async Task GetRole_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IGetRoleService>();
        service.Setup(s => s.GetRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleDto?)null);
        var controller = new GetRoleController(service.Object);

        // Act
        var result = await controller.GetRole(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
