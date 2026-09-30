using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Http;
using PolyBucket.Api.Features.Filaments.Queries;
using PolyBucket.Api.Features.Printers.Domain.Enums;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Filaments.GetFilamentById;

public class GetFilamentByIdControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When the filament exists, GetById returns Ok.")]
    public async Task GetById_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filament = new Filament { Id = id, Manufacturer = "A", Type = MaterialType.Pla, Color = "B", Diameter = "1.75" };
        _mediator.Setup(m => m.Send(It.IsAny<GetFilamentByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(filament);
        var controller = new GetFilamentByIdController(_mediator.Object);

        // Act
        var result = await controller.GetById(id);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(filament);
    }

    [Fact(DisplayName = "When the filament is missing, GetById returns NotFound.")]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetFilamentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Filament?)null);
        var controller = new GetFilamentByIdController(_mediator.Object);

        // Act
        var result = await controller.GetById(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
