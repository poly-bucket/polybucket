using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.CreateRole.Domain;
using PolyBucket.Api.Features.ACL.CreateRole.Http;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.CreateRole.Http;

public class CreateRoleControllerTests
{
    [Fact(DisplayName = "When creating a role without a user id, CreateRole returns Unauthorized.")]
    public async Task CreateRole_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var service = new Mock<ICreateRoleService>();
        service.Setup(s => s.CreateRoleAsync(Guid.Empty, It.IsAny<CreateRoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateRoleOutcome.Unauthorized, null, null));
        var controller = new CreateRoleController(service.Object).WithUser(null);
        var request = new CreateRoleRequest { Name = "Custom", Description = "Desc", Priority = 10 };

        // Act
        var result = await controller.CreateRole(request, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact(DisplayName = "When role creation fails validation, CreateRole returns BadRequest.")]
    public async Task CreateRole_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var service = new Mock<ICreateRoleService>();
        service.Setup(s => s.CreateRoleAsync(userId, It.IsAny<CreateRoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateRoleOutcome.Invalid, null, "Role name already exists"));
        var controller = new CreateRoleController(service.Object).WithUser(userId);

        // Act
        var result = await controller.CreateRole(new CreateRoleRequest { Name = "Dup", Description = "Desc", Priority = 10 }, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
