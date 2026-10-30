using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Http;
using PolyBucket.Storage;
using PolyBucket.Storage.GetStorageProviders;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Http;

public class GetStorageProvidersControllerTests
{
    [Fact(DisplayName = "When providers are requested, get storage providers returns Ok with catalog entries.")]
    public void GetStorageProviders_ReturnsOkWithCatalog()
    {
        // Arrange
        IGetStorageProvidersService service = new GetStorageProvidersService();
        var controller = new GetStorageProvidersController(service);

        // Act
        var result = controller.GetStorageProviders();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<StorageProvidersResponseDto>();
        ok.Providers.Count.ShouldBe(3);
        ok.Providers.Select(p => p.Id).ShouldBe([StorageProviderIds.Minio, StorageProviderIds.Silo, StorageProviderIds.SeaweedFs]);
    }
}
