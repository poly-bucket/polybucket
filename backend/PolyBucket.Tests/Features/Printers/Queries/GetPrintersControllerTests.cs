using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Printers.Domain;
using PolyBucket.Api.Features.Printers.Queries;
using PolyBucket.Api.Features.Printers.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Printers.Queries;

public class GetPrintersControllerTests
{
    [Fact(DisplayName = "When printers exist, get printers returns Ok with mapped DTOs.")]
    public async Task GetPrinters_ReturnsOk()
    {
        // Arrange
        var printerId = Guid.NewGuid();
        var repository = new Mock<IPrintersRepository>();
        repository.Setup(r => r.GetPrintersAsync()).ReturnsAsync(new List<Printer>
        {
            new()
            {
                Id = printerId,
                Manufacturer = "Bambu",
                Model = "X1",
                Type = PrinterType.FDM,
                Description = "Fast",
                PriceUSD = 999
            }
        });
        var handler = new GetPrintersQueryHandler(repository.Object);
        var controller = new GetPrintersController(handler);

        // Act
        var result = await controller.GetPrinters(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<GetPrintersResponse>();
        body.Printers.Count.ShouldBe(1);
        body.Printers[0].Manufacturer.ShouldBe("Bambu");
    }
}
