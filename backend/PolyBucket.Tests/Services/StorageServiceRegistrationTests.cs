using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Extensions;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Services;

public class StorageServiceRegistrationTests
{
    private IServiceProvider BuildServiceProvider(Dictionary<string, string> inMemorySettings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddObjectStorage(configuration);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("S3", typeof(AwsS3StorageService))]
    [InlineData("AWS", typeof(AwsS3StorageService))]
    [InlineData("AzureBlob", typeof(AzureBlobStorageService))]
    [InlineData("Azure", typeof(AzureBlobStorageService))]
    [InlineData("MinIO", typeof(MinioStorageService))]
    public void AddObjectStorage_RegistersCorrectImplementation(string providerValue, Type expectedType)
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            {"Storage:Provider", providerValue},
            {"Storage:BucketName", "unit-test"}
        };

        // Act
        var sp = BuildServiceProvider(settings);
        var storage = sp.GetRequiredService<IStorageService>();

        // Assert
        storage.ShouldNotBeNull();
        storage.ShouldBeAssignableTo(expectedType);
    }

    [Fact]
    public void AddObjectStorage_DefaultsToMinio_WhenProviderMissing()
    {
        var settings = new Dictionary<string, string>
        {
            {"Storage:BucketName", "unit-test"}
        };

        var sp = BuildServiceProvider(settings);
        var storage = sp.GetRequiredService<IStorageService>();

        storage.ShouldBeOfType<MinioStorageService>();
    }
} 