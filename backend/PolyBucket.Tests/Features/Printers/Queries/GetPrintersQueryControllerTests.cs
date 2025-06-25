using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Features.Printers.Queries;
using Shouldly;
using PolyBucket.Api.Features.Printers.Repository;
using System;
using System.Threading;
using System.Linq;

namespace PolyBucket.Tests.Features.Printers.Queries
{
    public class GetPrintersQueryControllerTests
    {
        private readonly Mock<GetPrintersQueryHandler> _mockHandler;
        private readonly GetPrintersQueryController _controller;

        public GetPrintersQueryControllerTests()
        {
            var mockRepo = new Mock<IPrintersRepository>();
            _mockHandler = new Mock<GetPrintersQueryHandler>(mockRepo.Object);
            _controller = new GetPrintersQueryController(_mockHandler.Object);
        }

        [Fact]
        public async Task GetPrinters_ReturnsListOfPrinters()
        {
            // Arrange
            var expectedPrinters = new List<PrinterDto>
            {
                new() { Manufacturer = "Prusa" },
                new() { Manufacturer = "Bambu" }
            };
            var expectedResponse = new GetPrintersResponse { Printers = expectedPrinters };
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetPrintersQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPrinters(default);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<GetPrintersResponse>();
            response.Printers.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetPrinters_NoPrinters_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = new GetPrintersResponse { Printers = new List<PrinterDto>() };
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetPrintersQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPrinters(default);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<GetPrintersResponse>();
            response.Printers.ShouldBeEmpty();
        }
    }
} 