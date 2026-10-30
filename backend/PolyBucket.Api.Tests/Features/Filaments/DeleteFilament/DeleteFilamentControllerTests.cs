using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.DeleteFilament.Domain;
using PolyBucket.Api.Features.Filaments.DeleteFilament.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Filaments.DeleteFilament;

public class DeleteFilamentControllerTests
{
    [Fact(DisplayName = "When deletion succeeds, Delete returns NoContent.")]
    public async Task Delete_Success_ReturnsNoContent()
    {
        // Arrange
        var service = new Mock<IDeleteFilamentService>();
        service.Setup(s => s.DeleteAsync(It.IsAny<DeleteFilamentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new DeleteFilamentController(service.Object);

        // Act
        var result = await controller.Delete(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When the filament is missing, Delete returns NotFound.")]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        // Arrange
        var service = new Mock<IDeleteFilamentService>();
        service.Setup(s => s.DeleteAsync(It.IsAny<DeleteFilamentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = new DeleteFilamentController(service.Object);

        // Act
        var result = await controller.Delete(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
