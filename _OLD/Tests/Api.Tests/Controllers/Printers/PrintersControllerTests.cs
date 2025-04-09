using api.Controllers.Printers.Domain;
using api.Controllers.Printers.Http;
using Core.Models.Enumerations.Printers;
using Core.Models.Printers;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace Tests.Api.Tests.Controllers.Printers;

public class PrintersControllerTests : IDisposable
{
    private readonly Mock<IGetPrintersService> _mockGetPrintersService;
    private readonly PrintersController _controller;
    private readonly Context _context;

    public PrintersControllerTests()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new Context(options);

        _mockGetPrintersService = new Mock<IGetPrintersService>();
        _controller = new PrintersController(_mockGetPrintersService.Object);
    }

    [Fact(DisplayName = "GetPrinters Returns List Of Printers (200)")]
    public async Task GetPrinters_ReturnsListOfPrinters()
    {
        // Arrange
        var expectedPrinters = new List<PrinterDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Manufacturer = "Prusa Research",
                Model = "i3 MK3S+",
                Type = PrinterType.FDM.ToString(),
                Description = "The Original Prusa i3 MK3S+ is the latest version of their award-winning 3D printer.",
                PriceUSD = 749m
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Manufacturer = "Bambu Lab",
                Model = "X1 Carbon",
                Type = PrinterType.FDM.ToString(),
                Description = "High-speed CoreXY printer with advanced features.",
                PriceUSD = 1199m
            }
        };

        var expectedResponse = new GetPrintersResponse
        {
            Printers = expectedPrinters
        };

        _mockGetPrintersService
            .Setup(x => x.ExecuteAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetPrinters();

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetPrintersResponse>();
        response.Printers.Count.ShouldBe(2);
        
        var firstPrinter = response.Printers[0];
        firstPrinter.Manufacturer.ShouldBe("Prusa Research");
        firstPrinter.Model.ShouldBe("i3 MK3S+");
        firstPrinter.Type.ShouldBe(PrinterType.FDM.ToString());
        firstPrinter.PriceUSD.ShouldBe(749m);

        var secondPrinter = response.Printers[1];
        secondPrinter.Manufacturer.ShouldBe("Bambu Lab");
        secondPrinter.Model.ShouldBe("X1 Carbon");
        secondPrinter.Type.ShouldBe(PrinterType.FDM.ToString());
        secondPrinter.PriceUSD.ShouldBe(1199m);
    }

    [Fact(DisplayName = "GetPrinters Returns Empty List When No Printers (200)")]
    public async Task GetPrinters_NoPrinters_ReturnsEmptyList()
    {
        // Arrange
        var expectedResponse = new GetPrintersResponse
        {
            Printers = new List<PrinterDto>()
        };

        _mockGetPrintersService
            .Setup(x => x.ExecuteAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetPrinters();

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetPrintersResponse>();
        response.Printers.Count.ShouldBe(0);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 