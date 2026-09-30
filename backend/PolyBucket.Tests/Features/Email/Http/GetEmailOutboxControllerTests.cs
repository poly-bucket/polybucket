using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class GetEmailOutboxControllerTests
{
    private readonly Mock<IGetEmailOutboxService> _service = new();
    private readonly GetEmailOutboxController _controller;

    public GetEmailOutboxControllerTests()
    {
        _controller = new GetEmailOutboxController(_service.Object);
    }

    [Fact(DisplayName = "When the outbox is requested with a status filter, the filter and paging are passed to the service.")]
    public async Task GetEmailOutbox_PassesFilterAndPaging()
    {
        // Arrange
        var page = new EmailOutboxPageDto { Page = 2, TotalCount = 30 };
        _service.Setup(s => s.GetAsync(EmailMessageStatus.DeadLetter, 2, 10, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        // Act
        var result = await _controller.GetEmailOutbox(EmailMessageStatus.DeadLetter, 2, 10);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(page);
    }

    [Fact(DisplayName = "When no filter is provided, GetEmailOutbox uses default paging.")]
    public async Task GetEmailOutbox_DefaultPaging_CallsService()
    {
        // Arrange
        var page = new EmailOutboxPageDto { Page = 1, TotalCount = 0 };
        _service.Setup(s => s.GetAsync(null, 1, 25, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        // Act
        var result = await _controller.GetEmailOutbox();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
        _service.Verify(s => s.GetAsync(null, 1, 25, It.IsAny<CancellationToken>()), Times.Once);
    }
}
