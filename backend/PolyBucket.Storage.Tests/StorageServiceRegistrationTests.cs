using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Storage.DependencyInjection;
using Shouldly;

namespace PolyBucket.Storage.Tests;

public class StorageServiceRegistrationTests
{
    private static Dictionary<string, string?> MinioSettings(string? provider = "MinIO") => new()
    {
        ["Storage:Provider"] = provider,
        ["Storage:Endpoint"] = "localhost",
        ["Storage:Port"] = "9000",
        ["Storage:AccessKey"] = "key",
        ["Storage:SecretKey"] = "secret",
        ["Storage:BucketName"] = "unit-test"
    };

    private ServiceCollection BuildServices(Dictionary<string, string?> inMemorySettings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddObjectStorage(configuration);
        return services;
    }

    [Theory]
    [InlineData("MinIO")]
    [InlineData("minio")]
    public void AddObjectStorage_RegistersRoutingStorageService(string providerValue)
    {
        var services = BuildServices(MinioSettings(providerValue));

        services.ShouldContain(d =>
            d.ServiceType == typeof(IStorageService) && d.ImplementationType == typeof(RoutingStorageService));
    }

    [Fact]
    public void AddObjectStorage_AllowsEmptyEnvConfiguration()
    {
        var services = BuildServices(new Dictionary<string, string?>());

        services.ShouldContain(d => d.ServiceType == typeof(IStorageSettingsResolver));
        services.ShouldContain(d => d.ServiceType == typeof(IStorageService));
    }

    [Theory]
    [InlineData("S3")]
    [InlineData("Azure")]
    public void AddObjectStorage_Throws_WhenProviderIsNotMinio(string provider)
    {
        var settings = MinioSettings(provider);
        Should.Throw<InvalidOperationException>(() => BuildServices(settings));
    }
}
