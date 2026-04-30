using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Extensions;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Services;

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

    private IServiceProvider BuildServiceProvider(Dictionary<string, string?> inMemorySettings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddObjectStorage(configuration);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("MinIO")]
    [InlineData("minio")]
    public void AddObjectStorage_RegistersMinioImplementation(string providerValue)
    {
        var sp = BuildServiceProvider(MinioSettings(providerValue));
        var storage = sp.GetRequiredService<IStorageService>();

        storage.ShouldNotBeNull();
        storage.ShouldBeOfType<MinioStorageService>();
    }

    [Fact]
    public void AddObjectStorage_DefaultsToMinio_WhenProviderMissing()
    {
        var settings = MinioSettings(null);
        settings.Remove("Storage:Provider");

        var sp = BuildServiceProvider(settings);
        var storage = sp.GetRequiredService<IStorageService>();

        storage.ShouldBeOfType<MinioStorageService>();
    }

    [Theory]
    [InlineData("S3")]
    [InlineData("Azure")]
    public void AddObjectStorage_Throws_WhenProviderIsNotMinio(string provider)
    {
        var settings = MinioSettings(provider);
        Should.Throw<InvalidOperationException>(() => BuildServiceProvider(settings));
    }
}
