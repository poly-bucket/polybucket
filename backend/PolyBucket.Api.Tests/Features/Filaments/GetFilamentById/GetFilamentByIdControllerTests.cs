using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.GetFilamentById.Domain;
using PolyBucket.Api.Features.Filaments.GetFilamentById.Http;
using PolyBucket.Database.Entities.Printers.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Filaments.GetFilamentById;

public class GetFilamentByIdControllerTests
{
    [Fact(DisplayName = "When the filament exists, GetById returns Ok.")]
    public async Task GetById_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filament = new Filament { Id = id, Manufacturer = "A", Type = MaterialType.Pla, Color = "B", Diameter = "1.75" };
        var service = new Mock<IGetFilamentByIdService>();
        service.Setup(s => s.GetByIdAsync(It.IsAny<GetFilamentByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(filament);
        var controller = new GetFilamentByIdController(service.Object);

        // Act
        var result = await controller.GetById(id);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(filament);
    }

    [Fact(DisplayName = "When the filament is missing, GetById returns NotFound.")]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IGetFilamentByIdService>();
        service.Setup(s => s.GetByIdAsync(It.IsAny<GetFilamentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Filament?)null);
        var controller = new GetFilamentByIdController(service.Object);

        // Act
        var result = await controller.GetById(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
