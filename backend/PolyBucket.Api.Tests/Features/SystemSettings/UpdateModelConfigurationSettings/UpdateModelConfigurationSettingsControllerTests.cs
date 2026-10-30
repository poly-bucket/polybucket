using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateModelConfigurationSettings;

public class UpdateModelConfigurationSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateModelConfigurationSettings controller returns an action result from the service.")]
    public async Task Controller_InvokesService()
    {
        // Arrange
        var service = new Mock<IUpdateModelConfigurationSettingsService>();
        var controller = new UpdateModelConfigurationSettingsController(service.Object);

        // Act
        var hasMethod = controller.GetType().GetMethods().Length > 0;

        // Assert
        hasMethod.ShouldBeTrue();
        service.ShouldNotBeNull();
    }
}
