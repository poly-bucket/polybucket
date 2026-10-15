using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class SystemSetupControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "Get setup status returns Ok with the mediator response.")]
    public async Task GetSetupStatus_ReturnsOk()
    {
        // Arrange
        var response = new CheckFirstTimeSetupResponse { IsFirstTimeSetup = true };
        _mediator.SetupSend<CheckFirstTimeSetupQuery, CheckFirstTimeSetupResponse>(response);
        var controller = new SystemSetupController(_mediator.Object);

        // Act
        var result = await controller.GetSetupStatus();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "Get site settings as admin returns Ok with the mediator response.")]
    public async Task GetSiteSettings_Admin_ReturnsOk()
    {
        // Arrange
        var response = new GetSiteSettingsResponse
        {
            Success = true,
            Settings = new SiteSettingsData { SiteName = "Test" }
        };
        _mediator.SetupSend<GetSiteSettingsQuery, GetSiteSettingsResponse>(response);
        var controller = new SystemSetupController(_mediator.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.GetSiteSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "Get site settings without admin role returns Forbid.")]
    public async Task GetSiteSettings_NonAdmin_ReturnsForbid()
    {
        // Arrange
        var controller = new SystemSetupController(_mediator.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetSiteSettings();

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
        _mediator.Verify(m => m.Send(It.IsAny<IRequest<GetSiteSettingsResponse>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Complete setup without admin role returns Forbid.")]
    public async Task CompleteSetup_NonAdmin_ReturnsForbid()
    {
        // Arrange
        var controller = new SystemSetupController(_mediator.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.CompleteSetup();

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
        _mediator.Verify(m => m.Send(It.IsAny<IRequest<CompleteFirstTimeSetupResponse>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
