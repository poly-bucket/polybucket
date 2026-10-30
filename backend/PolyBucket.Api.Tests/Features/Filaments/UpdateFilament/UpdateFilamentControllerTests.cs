using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.UpdateFilament.Domain;
using PolyBucket.Api.Features.Filaments.UpdateFilament.Http;
using PolyBucket.Database.Entities.Printers.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Filaments.UpdateFilament;

public class UpdateFilamentControllerTests
{
    [Fact(DisplayName = "When the filament is updated, Update returns Ok.")]
    public async Task Update_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateFilamentCommand
        {
            Manufacturer = "New",
            Type = MaterialType.Pla,
            Color = "B",
            Diameter = "1.75"
        };
        var filament = new Filament { Id = id, Manufacturer = "New", Type = MaterialType.Pla, Color = "B", Diameter = "1.75" };
        var service = new Mock<IUpdateFilamentService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateFilamentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(filament);
        var controller = new UpdateFilamentController(service.Object);

        // Act
        var result = await controller.Update(id, command);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(filament);
        command.Id.ShouldBe(id);
    }

    [Fact(DisplayName = "When the filament is missing, Update returns NotFound.")]
    public async Task Update_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IUpdateFilamentService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateFilamentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Filament?)null);
        var controller = new UpdateFilamentController(service.Object);

        // Act
        var result = await controller.Update(Guid.NewGuid(), new UpdateFilamentCommand
        {
            Manufacturer = "X",
            Type = MaterialType.Pla,
            Color = "B",
            Diameter = "1.75"
        });

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
