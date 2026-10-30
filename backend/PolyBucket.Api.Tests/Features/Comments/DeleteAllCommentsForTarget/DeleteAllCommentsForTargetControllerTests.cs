using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DeleteAllCommentsForTarget;

public class DeleteAllCommentsForTargetControllerTests
{
    private readonly Mock<IDeleteAllCommentsForTargetService> _service = new();

    [Fact(DisplayName = "When deletion succeeds, the controller returns Ok.")]
    public async Task DeleteAllCommentsForTarget_Success_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.DeleteAllAsync("model", It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAllCommentsForTargetResult(true, null));
        var controller = new DeleteAllCommentsForTargetController(_service.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.DeleteAllCommentsForTarget("model", Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
