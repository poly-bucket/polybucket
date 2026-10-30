using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.CreateFilament.Domain;
using PolyBucket.Api.Features.Filaments.CreateFilament.Http;
using PolyBucket.Database.Entities.Printers.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Filaments.CreateFilament;

public class CreateFilamentControllerTests
{
    [Fact(DisplayName = "When a filament is created, Create returns 201 with the filament.")]
    public async Task Create_ReturnsCreated()
    {
        // Arrange
        var command = new CreateFilamentCommand
        {
            Manufacturer = "Acme",
            Type = MaterialType.Pla,
            Color = "Red",
            Diameter = "1.75"
        };
        var filament = new Filament { Manufacturer = "Acme", Type = MaterialType.Pla, Color = "Red", Diameter = "1.75" };
        var service = new Mock<ICreateFilamentService>();
        service.Setup(s => s.CreateAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(filament);
        var controller = new CreateFilamentController(service.Object);

        // Act
        var result = await controller.Create(command);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>().Value.ShouldBe(filament);
    }
}
