using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.GetAllFilaments.Domain;
using PolyBucket.Api.Features.Filaments.GetAllFilaments.Http;
using PolyBucket.Database.Entities.Printers.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Filaments.GetAllFilaments;

public class GetAllFilamentsControllerTests
{
    [Fact(DisplayName = "When filaments exist, GetAll returns Ok with the list.")]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var filaments = new List<Filament>
        {
            new() { Manufacturer = "A", Type = MaterialType.Pla, Color = "B", Diameter = "1.75" }
        };
        var service = new Mock<IGetAllFilamentsService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(filaments);
        var controller = new GetAllFilamentsController(service.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(filaments);
    }
}
