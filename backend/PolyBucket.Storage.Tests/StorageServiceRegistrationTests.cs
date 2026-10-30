using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Storage;
using PolyBucket.Storage.DependencyInjection;
using Shouldly;

namespace PolyBucket.Storage.Tests;

public class StorageServiceRegistrationTests
{
    private static Dictionary<string, string?> CompleteEnvSettings(string provider) => new()
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
    [InlineData(StorageProviderIds.Minio)]
    [InlineData(StorageProviderIds.Silo)]
    public void AddObjectStorage_RegistersRoutingStorageService(string providerValue)
    {
        var services = BuildServices(CompleteEnvSettings(providerValue));

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

    [Fact]
    public void AddObjectStorage_DoesNotThrow_ForUnsupportedLegacyEnvProvider()
    {
        var services = BuildServices(CompleteEnvSettings("S3"));

        services.ShouldContain(d => d.ServiceType == typeof(IStorageService));
    }
}
