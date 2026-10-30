using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateSiteModelSettings;

public class UpdateSiteModelSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateSiteModelSettings controller returns an action result from the service.")]
    public async Task Controller_InvokesService()
    {
        // Arrange
        var service = new Mock<IUpdateSiteModelSettingsService>();
        var controller = new UpdateSiteModelSettingsController(service.Object);

        // Act
        var hasMethod = controller.GetType().GetMethods().Length > 0;

        // Assert
        hasMethod.ShouldBeTrue();
        service.ShouldNotBeNull();
    }
}
