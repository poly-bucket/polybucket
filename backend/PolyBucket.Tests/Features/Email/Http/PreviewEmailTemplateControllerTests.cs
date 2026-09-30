using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Domain;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class PreviewEmailTemplateControllerTests
{
    private readonly Mock<IPreviewEmailTemplateService> _service = new();
    private readonly PreviewEmailTemplateController _controller;

    public PreviewEmailTemplateControllerTests()
    {
        _controller = new PreviewEmailTemplateController(_service.Object);
    }

    [Fact(DisplayName = "When a template is previewed, the controller returns 200 with the rendered email.")]
    public async Task PreviewEmailTemplate_ReturnsOk()
    {
        // Arrange
        var rendered = new RenderedEmail("Subject", "<p>Html</p>", "Text");
        _service.Setup(s => s.PreviewAsync(EmailTemplateKey.Welcome, It.IsAny<CancellationToken>())).ReturnsAsync(rendered);

        // Act
        var result = await _controller.PreviewEmailTemplate(EmailTemplateKey.Welcome);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(rendered);
    }

    [Fact(DisplayName = "When the template does not exist, the controller returns 404.")]
    public async Task PreviewEmailTemplate_Unknown_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.PreviewAsync(It.IsAny<EmailTemplateKey>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException("missing"));

        // Act
        var result = await _controller.PreviewEmailTemplate((EmailTemplateKey)999);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
