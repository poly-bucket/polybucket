using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;
using PolyBucket.Api.Features.Email.RetryEmailMessage.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class RetryEmailMessageControllerTests
{
    private readonly Mock<IRetryEmailMessageService> _service = new();
    private readonly RetryEmailMessageController _controller;

    public RetryEmailMessageControllerTests()
    {
        _controller = new RetryEmailMessageController(_service.Object);
    }

    [Fact(DisplayName = "When the message is requeued, the controller returns 204.")]
    public async Task RetryEmailMessage_Success_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.RetryEmailMessage(id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _service.Verify(s => s.RetryAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the message does not exist, the controller returns 404.")]
    public async Task RetryEmailMessage_NotFound_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.RetryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException("missing"));

        // Act
        var result = await _controller.RetryEmailMessage(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the message was already sent or is pending, the controller returns 409.")]
    public async Task RetryEmailMessage_NotRetryable_ReturnsConflict()
    {
        // Arrange
        _service.Setup(s => s.RetryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ConflictException("already sent"));

        // Act
        var result = await _controller.RetryEmailMessage(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<ConflictObjectResult>();
    }
}
