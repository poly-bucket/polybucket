using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.ReportComment.Domain;
using PolyBucket.Api.Features.Comments.ReportComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ReportComment;

public class ReportCommentControllerTests
{
    private readonly Mock<IReportCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a report is submitted, the controller returns Ok.")]
    public async Task ReportComment_Success_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.ReportAsync(It.IsAny<Guid>(), _userId, "spam", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new ReportCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.ReportComment(Guid.NewGuid(), new ReportCommentRequest { Reason = "spam" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
