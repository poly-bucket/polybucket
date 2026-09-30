using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Http;
using PolyBucket.Api.Features.Printers.Domain.Enums;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Filaments.CreateFilament;

public class CreateFilamentControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

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
        _mediator.SetupSend<CreateFilamentCommand, Filament>(filament);
        var controller = new CreateFilamentController(_mediator.Object);

        // Act
        var result = await controller.Create(command);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>().Value.ShouldBe(filament);
    }
}
