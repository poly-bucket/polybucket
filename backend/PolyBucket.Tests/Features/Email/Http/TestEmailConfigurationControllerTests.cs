using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class TestEmailConfigurationControllerTests
{
    private readonly Mock<ITestEmailConfigurationService> _service = new();
    private readonly TestEmailConfigurationController _controller;

    public TestEmailConfigurationControllerTests()
    {
        _controller = new TestEmailConfigurationController(_service.Object);
    }

    [Fact(DisplayName = "When the test fails at a stage, the controller still returns 200 so the UI can show the stage breakdown.")]
    public async Task TestEmailConfiguration_Failure_ReturnsOkWithStages()
    {
        // Arrange
        var failure = new EmailTestResult(false, "Authentication failed", [new EmailDiagnosticStageResult(EmailDiagnosticStage.Auth, false, "Authentication failed", 12)]);
        _service.Setup(s => s.TestAsync("admin@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(failure);

        // Act
        var result = await _controller.TestEmailConfiguration(new TestEmailConfigurationRequest { TestEmailAddress = "admin@example.com" });

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<EmailTestResult>();
        body.Success.ShouldBeFalse();
        body.Stages.ShouldHaveSingleItem().Stage.ShouldBe(EmailDiagnosticStage.Auth);
    }

    [Fact(DisplayName = "When the test address is invalid, the controller returns 400 without sending.")]
    public async Task TestEmailConfiguration_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(TestEmailConfigurationRequest.TestEmailAddress), "Invalid email");

        // Act
        var result = await _controller.TestEmailConfiguration(new TestEmailConfigurationRequest { TestEmailAddress = "nope" });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.TestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
