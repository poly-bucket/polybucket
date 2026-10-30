using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Mvc;
        using Moq;
        using PolyBucket.Api.Features.Printers.GetPrinters.Domain;
        using PolyBucket.Api.Features.Printers.GetPrinters.Http;
        using Shouldly;
        using Xunit;

        namespace PolyBucket.Api.Tests.Features.Printers.GetPrinters.Http;

        public class GetPrintersControllerTests
        {
            private readonly Mock<IGetPrintersService> _service = new();

            [Fact(DisplayName = "When the service succeeds, GetPrinters returns Ok.")]
            public async Task GetPrinters_ReturnsOk()
            {
                // Arrange
                var payload = new GetPrintersResponseDto
                {
                    Printers = { new PrinterListItemDto { Manufacturer = "Bambu", Model = "X1", Type = "FDM", Description = "d" } }
                };
                _service.Setup(s => s.GetPrintersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(payload);
                var controller = new GetPrintersController(_service.Object);

                // Act
                var result = await controller.GetPrinters(CancellationToken.None);

                // Assert
                var ok = result.Result.ShouldBeOfType<OkObjectResult>();
                ok.Value.ShouldBeOfType<GetPrintersResponseDto>().Printers.Count.ShouldBe(1);
            }
        }
