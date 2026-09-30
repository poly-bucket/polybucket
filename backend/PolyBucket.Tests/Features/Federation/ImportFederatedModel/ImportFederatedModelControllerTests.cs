using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.ImportFederatedModel;

public class ImportFederatedModelControllerTests
{
    private readonly Mock<IFederationImportService> _importService = new();

    [Fact(DisplayName = "When instance id is missing, ImportFederatedModel returns BadRequest.")]
    public async Task ImportFederatedModel_MissingInstance_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ImportFederatedModelController(_importService.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.ImportFederatedModel(new ImportFederatedModelRequest { RemoteModelId = "rm-1" });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When import succeeds, ImportFederatedModel returns 201.")]
    public async Task ImportFederatedModel_Success_ReturnsCreated()
    {
        // Arrange
        var model = new Model { Id = Guid.NewGuid(), Name = "Imported", Description = "D", IsFederated = true };
        _importService.Setup(s => s.ImportModelAsync("inst", "rm-1")).ReturnsAsync(model);
        var controller = new ImportFederatedModelController(_importService.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.ImportFederatedModel(new ImportFederatedModelRequest
        {
            InstanceId = "inst",
            RemoteModelId = "rm-1"
        });

        // Assert
        result.Result.ShouldBeOfType<CreatedAtActionResult>();
    }
}
