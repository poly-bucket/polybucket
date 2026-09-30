using System;
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

namespace PolyBucket.Tests.Features.Filaments.UpdateFilament;

public class UpdateFilamentControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

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
        _mediator.SetupSend<UpdateFilamentCommand, Filament>(filament);
        var controller = new UpdateFilamentController(_mediator.Object);

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
        _mediator.Setup(m => m.Send(It.IsAny<UpdateFilamentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Filament?)null);
        var controller = new UpdateFilamentController(_mediator.Object);

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
