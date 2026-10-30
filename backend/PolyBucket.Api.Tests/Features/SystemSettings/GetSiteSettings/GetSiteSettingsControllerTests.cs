using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetSiteSettings;

public class GetSiteSettingsControllerTests
{
    [Fact(DisplayName = "Get site settings without admin role returns Forbid.")]
    public async Task GetSiteSettings_NonAdmin_ReturnsForbid()
    {
        // Arrange
        var service = new Mock<IGetSiteSettingsService>();
        var controller = new GetSiteSettingsController(service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetSiteSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
    }
}
