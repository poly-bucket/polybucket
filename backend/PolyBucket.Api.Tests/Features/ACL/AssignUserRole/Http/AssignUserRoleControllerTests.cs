using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.AssignUserRole.Domain;
using PolyBucket.Api.Features.ACL.AssignUserRole.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.AssignUserRole.Http;

public class AssignUserRoleControllerTests
{
    [Fact(DisplayName = "When the assigner cannot manage the role, AssignUserRole returns Forbid.")]
    public async Task AssignUserRole_CannotManageRole_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var service = new Mock<IAssignUserRoleService>();
        service.Setup(s => s.AssignUserRoleAsync(userId, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssignUserRoleOutcome.Forbidden, "Insufficient privileges to assign this role"));
        var controller = new AssignUserRoleController(service.Object).WithUser(userId);

        // Act
        var result = await controller.AssignUserRole(Guid.NewGuid(), new AssignUserRoleRequest { RoleId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ForbidResult>();
    }
}
