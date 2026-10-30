using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.GetAllRoles.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.GetAllRoles.Http;

public class GetAllRolesControllerTests
{
    [Fact(DisplayName = "When roles are requested, GetAllRoles returns Ok.")]
    public async Task GetAllRoles_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetAllRolesService>();
        service.Setup(s => s.GetAllRolesAsync(1, 10, null, "priority", true))
            .ReturnsAsync(new PaginatedRolesResponse());
        var controller = new GetAllRolesController(service.Object);

        // Act
        var result = await controller.GetAllRoles();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
