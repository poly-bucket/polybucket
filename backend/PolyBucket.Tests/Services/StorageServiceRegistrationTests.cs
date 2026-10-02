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
    private static Dictionary<string, string?> S3Settings(string? provider = "S3") => new()
    {
        ["Storage:Provider"] = provider,
        ["Storage:Endpoint"] = "localhost",
        ["Storage:Port"] = "8333",
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
        services.AddLogging();
        services.AddObjectStorage(configuration);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("S3")]
    [InlineData("s3")]
    [InlineData("SeaweedFS")]
    public void AddObjectStorage_RegistersS3Implementation(string providerValue)
    {
        var sp = BuildServiceProvider(S3Settings(providerValue));
        var storage = sp.GetRequiredService<IStorageService>();

        storage.ShouldNotBeNull();
        storage.ShouldBeOfType<AwsS3StorageService>();
        sp.GetRequiredService<IStorageObjectKeyResolver>().ShouldNotBeNull();
    }

    [Fact]
    public void AddObjectStorage_DefaultsToS3_WhenProviderMissing()
    {
        var settings = S3Settings(null);
        settings.Remove("Storage:Provider");

        var sp = BuildServiceProvider(settings);
        var storage = sp.GetRequiredService<IStorageService>();

        storage.ShouldBeOfType<AwsS3StorageService>();
    }

    [Theory]
    [InlineData("MinIO")]
    [InlineData("Azure")]
    public void AddObjectStorage_Throws_WhenProviderIsNotSupported(string provider)
    {
        var settings = S3Settings(provider);
        Should.Throw<InvalidOperationException>(() => BuildServiceProvider(settings));
    }
}
