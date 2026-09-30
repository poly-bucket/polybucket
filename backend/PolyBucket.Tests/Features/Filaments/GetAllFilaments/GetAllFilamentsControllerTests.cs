using System.Collections.Generic;
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

namespace PolyBucket.Tests.Features.Filaments.GetAllFilaments;

public class GetAllFilamentsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When filaments exist, GetAll returns Ok with the list.")]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var filaments = new List<Filament>
        {
            new() { Manufacturer = "A", Type = MaterialType.Pla, Color = "B", Diameter = "1.75" }
        };
        _mediator.SetupSend<GetAllFilamentsQuery, List<Filament>>(filaments);
        var controller = new GetAllFilamentsController(_mediator.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(filaments);
    }
}
