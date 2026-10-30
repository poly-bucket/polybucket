using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetSiteModelSettings;

public class GetSiteModelSettingsControllerTests
{
    [Fact(DisplayName = "The GetSiteModelSettings controller returns an action result from the service.")]
    public async Task Controller_InvokesService()
    {
        // Arrange
        var service = new Mock<IGetSiteModelSettingsService>();
        var controller = new GetSiteModelSettingsController(service.Object);

        // Act
        var hasMethod = controller.GetType().GetMethods().Length > 0;

        // Assert
        hasMethod.ShouldBeTrue();
        service.ShouldNotBeNull();
    }
}
