using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.DownloadModel.Domain;
using PolyBucket.Api.Features.Models.DownloadModel.Http;
using PolyBucket.Api.Features.Models.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DownloadModel.Http;

public class DownloadModelControllerTests
{
    [Fact(DisplayName = "When downloading a model and the service returns NotFound, the download model controller returns NotFound.")]
    public async Task DownloadModel_WhenServiceReturnsNotFound_ReturnsNotFound()
    {
        var mock = new Mock<IDownloadModelService>();
        mock
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DownloadModelOutcome.NotFound());

        var controller = new DownloadModelController(mock.Object);
        var id = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test"))
            }
        };

        var result = await controller.DownloadModel(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Model not found", notFound.Value);
    }

    [Fact(DisplayName = "When downloading a model and the service returns a single file, the download model controller returns the file stream result.")]
    public async Task DownloadModel_WhenServiceReturnsOkSingleFile_ReturnsFile()
    {
        var stream = new MemoryStream([1, 2, 3]);
        var mock = new Mock<IDownloadModelService>();
        mock
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DownloadModelOutcome.OkSingle(stream, "model/stl", "x.stl", ownerDisposes: true));

        var controller = new DownloadModelController(mock.Object);
        var id = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test"))
            }
        };

        var result = await controller.DownloadModel(id, CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("model/stl", file.ContentType);
        Assert.Equal("x.stl", file.FileDownloadName);
    }

    [Fact(DisplayName = "When the download outcome includes counts, the controller sets download count headers.")]
    public async Task DownloadModel_WhenOutcomeHasCounts_SetsHeaders()
    {
        var stream = new MemoryStream([1, 2, 3]);
        var mock = new Mock<IDownloadModelService>();
        mock
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DownloadModelOutcome.OkSingle(stream, "model/stl", "x.stl", ownerDisposes: true, downloads: 9, counted: true));

        var controller = new DownloadModelController(mock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test"))
            }
        };

        await controller.DownloadModel(Guid.NewGuid(), CancellationToken.None);

        controller.Response.Headers[ModelDownloadCountHeaders.Downloads].ToString().ShouldBe("9");
        controller.Response.Headers[ModelDownloadCountHeaders.Counted].ToString().ShouldBe("true");
    }

    [Fact(DisplayName = "When downloading a model and the service returns Forbid, the download model controller returns 403.")]
    public async Task DownloadModel_WhenServiceReturnsForbid_Returns403()
    {
        // Arrange
        var mock = new Mock<IDownloadModelService>();
        mock
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DownloadModelOutcome.Forbid());
        var controller = new DownloadModelController(mock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test")) }
        };

        // Act
        var result = await controller.DownloadModel(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact(DisplayName = "When downloading a model and the service returns a zip, the download model controller returns the zip file.")]
    public async Task DownloadModel_WhenServiceReturnsOkZip_ReturnsZip()
    {
        // Arrange
        var mock = new Mock<IDownloadModelService>();
        mock
            .Setup(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DownloadModelOutcome.OkZipFile([1, 2, 3], "bundle.zip"));
        var controller = new DownloadModelController(mock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Test")) }
        };

        // Act
        var result = await controller.DownloadModel(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/zip", file.ContentType);
        Assert.Equal("bundle.zip", file.FileDownloadName);
    }
}
