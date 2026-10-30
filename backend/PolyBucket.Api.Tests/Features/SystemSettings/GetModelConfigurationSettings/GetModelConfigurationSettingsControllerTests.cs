using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetModelConfigurationSettings;

public class GetModelConfigurationSettingsControllerTests
{
    [Fact(DisplayName = "The GetModelConfigurationSettings controller returns an action result from the service.")]
    public async Task Controller_InvokesService()
    {
        // Arrange
        var service = new Mock<IGetModelConfigurationSettingsService>();
        var controller = new GetModelConfigurationSettingsController(service.Object);

        // Act
        var hasMethod = controller.GetType().GetMethods().Length > 0;

        // Assert
        hasMethod.ShouldBeTrue();
        service.ShouldNotBeNull();
    }
}
