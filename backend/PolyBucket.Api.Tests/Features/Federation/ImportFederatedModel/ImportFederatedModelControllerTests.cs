using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Domain;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.ImportFederatedModel;

public class ImportFederatedModelControllerTests
{
    private readonly Mock<IImportFederatedModelService> _service = new();

    [Fact]
    public async Task Import_WhenInvalid_ReturnsBadRequest()
    {
        _service.Setup(s => s.ImportFederatedModelAsync(It.IsAny<ImportFederatedModelRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BadRequestObjectResult("bad"));
        var controller = new ImportFederatedModelController(_service.Object).WithUser(Guid.NewGuid());
        var result = await controller.ImportFederatedModel(new ImportFederatedModelRequest());
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
