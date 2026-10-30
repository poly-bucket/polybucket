using PolyBucket.Storage;
using Shouldly;

namespace PolyBucket.Storage.Tests;

public class StorageProviderCatalogTests
{
    [Theory]
    [InlineData("MinIO", StorageProviderIds.Minio)]
    [InlineData("SILO", StorageProviderIds.Silo)]
    [InlineData("seaweedfs", StorageProviderIds.SeaweedFs)]
    public void TryNormalize_AcceptsSupportedProviders(string input, string expected)
    {
        // Act
        var success = StorageProviderIds.TryNormalize(input, out var normalized);

        // Assert
        success.ShouldBeTrue();
        normalized.ShouldBe(expected);
    }

    [Fact]
    public void TryNormalize_RejectsUnsupportedProvider()
    {
        // Act
        var success = StorageProviderIds.TryNormalize("aws", out _);

        // Assert
        success.ShouldBeFalse();
    }

    [Fact]
    public void GetAll_ReturnsThreeProviders()
    {
        // Act
        var providers = StorageProviderCatalog.GetAll();

        // Assert
        providers.Count.ShouldBe(3);
    }

    [Fact]
    public void SeaweedFsDefaults_EnablePathStyle()
    {
        // Act
        var definition = StorageProviderCatalog.TryGet(StorageProviderIds.SeaweedFs);

        // Assert
        definition.ShouldNotBeNull();
        definition!.Defaults.ForcePathStyle.ShouldBeTrue();
        definition.Defaults.Port.ShouldBe(8333);
    }
}
