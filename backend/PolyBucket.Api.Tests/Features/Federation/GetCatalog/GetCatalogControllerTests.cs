using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.GetCatalog.Domain;
using PolyBucket.Api.Features.Federation.GetCatalog.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.GetCatalog;

public class GetCatalogControllerTests
{
    private readonly Mock<IGetCatalogService> _service = new();

    private GetCatalogController CreateController(string? bearer = null)
    {
        var controller = new GetCatalogController(_service.Object);
        var context = new DefaultHttpContext();
        if (bearer != null)
        {
            context.Request.Headers.Authorization = $"Bearer {bearer}";
        }
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    [Fact(DisplayName = "When no federation token is provided, GetCatalog returns Unauthorized.")]
    public async Task GetCatalog_NoToken_ReturnsUnauthorized()
    {
        _service.Setup(s => s.GetCatalogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnauthorizedObjectResult("Federation token required"));

        var result = await CreateController().GetCatalog();

        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When a federation token is provided, GetCatalog returns Ok with catalog metadata.")]
    public async Task GetCatalog_WithToken_ReturnsOk()
    {
        _service.Setup(s => s.GetCatalogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OkObjectResult(new CatalogResponse { TotalModels = 0 }));

        var result = await CreateController("fed-token").GetCatalog();

        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
