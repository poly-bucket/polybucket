using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Filaments.DeleteFilament;

public class DeleteFilamentControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When deletion succeeds, Delete returns NoContent.")]
    public async Task Delete_Success_ReturnsNoContent()
    {
        // Arrange
        _mediator.SetupSend<DeleteFilamentCommand, bool>(true);
        var controller = new DeleteFilamentController(_mediator.Object);

        // Act
        var result = await controller.Delete(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When the filament is missing, Delete returns NotFound.")]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        // Arrange
        _mediator.SetupSend<DeleteFilamentCommand, bool>(false);
        var controller = new DeleteFilamentController(_mediator.Object);

        // Act
        var result = await controller.Delete(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
