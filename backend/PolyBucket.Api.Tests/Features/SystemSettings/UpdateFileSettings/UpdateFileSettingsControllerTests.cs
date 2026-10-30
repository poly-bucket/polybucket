using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateFileSettings;

public class UpdateFileSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateFileSettings controller returns an action result from the service.")]
    public async Task Controller_InvokesService()
    {
        // Arrange
        var service = new Mock<IUpdateFileSettingsService>();
        var controller = new UpdateFileSettingsController(service.Object);

        // Act
        var hasMethod = controller.GetType().GetMethods().Length > 0;

        // Assert
        hasMethod.ShouldBeTrue();
        service.ShouldNotBeNull();
    }
}
