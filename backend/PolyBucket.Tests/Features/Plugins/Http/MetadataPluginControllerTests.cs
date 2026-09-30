using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins.Http;

public class MetadataPluginControllerTests
{
    [Fact(DisplayName = "When metadata plugins are registered, GetMetadataPlugins returns Ok.")]
    public void GetMetadataPlugins_ReturnsOk()
    {
        // Arrange
        var service = new MetadataPluginService(NullLogger<MetadataPluginService>.Instance);
        var controller = new MetadataPluginController(service, NullLogger<MetadataPluginController>.Instance);

        // Act
        var result = controller.GetMetadataPlugins();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeAssignableTo<List<MetadataPluginInfo>>();
    }

    [Fact(DisplayName = "When entity type is empty, GetEntityFields returns BadRequest.")]
    public async Task GetEntityFields_EmptyEntityType_ReturnsBadRequest()
    {
        // Arrange
        var service = new MetadataPluginService(NullLogger<MetadataPluginService>.Instance);
        var controller = new MetadataPluginController(service, NullLogger<MetadataPluginController>.Instance);

        // Act
        var result = await controller.GetEntityFields(" ");

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When field values are missing, ValidateMetadata returns BadRequest.")]
    public async Task ValidateMetadata_MissingFieldValues_ReturnsBadRequest()
    {
        // Arrange
        var service = new MetadataPluginService(NullLogger<MetadataPluginService>.Instance);
        var controller = new MetadataPluginController(service, NullLogger<MetadataPluginController>.Instance);

        // Act
        var result = await controller.ValidateMetadata("model", "id-1", new MetadataValidationRequest { FieldValues = null! });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
