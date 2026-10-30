using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Domain;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetModeratedComments;

public class GetModeratedCommentsControllerTests
{
    private readonly Mock<IGetModeratedCommentsService> _service = new();

    [Fact(DisplayName = "When paging is valid, moderated comments are returned.")]
    public async Task GetModeratedComments_Valid_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.GetAsync(1, 20, It.IsAny<System.Guid?>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetModeratedCommentsResult(new System.Collections.Generic.List<PolyBucket.Api.Features.Comments.Plugins.CommentResponse>(), null));
        var controller = new GetModeratedCommentsController(_service.Object).WithUser(System.Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.GetModeratedComments(cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
