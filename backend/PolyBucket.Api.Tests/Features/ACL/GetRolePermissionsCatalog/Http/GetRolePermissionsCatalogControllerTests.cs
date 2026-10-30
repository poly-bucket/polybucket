using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Domain;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ACL.GetRolePermissionsCatalog.Http;

public class GetRolePermissionsCatalogControllerTests
{
    [Fact(DisplayName = "When permissions are listed, GetAllPermissions returns categorized DTOs.")]
    public async Task GetAllPermissions_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetRolePermissionsCatalogService>();
        service.Setup(s => s.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PermissionCatalogItemDto> { new() { Name = "admin.manage_users", Category = "Administration" } });
        var controller = new GetRolePermissionsCatalogController(service.Object);

        // Act
        var result = await controller.GetAllPermissions(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var list = ok.Value.ShouldBeOfType<List<PermissionCatalogItemDto>>();
        list[0].Name.ShouldBe("admin.manage_users");
    }
}
