using Microsoft.Extensions.Options;
using PolyBucket.Storage;
using PolyBucket.Storage.Settings;
using Shouldly;

namespace PolyBucket.Storage.Tests;

public class StorageSettingsValidatorTests
{
    private readonly StorageSettingsValidator _validator = new();

    private static StorageSettings ValidSettings(string provider = StorageProviderIds.Minio) => new()
    {
        Provider = provider,
        Endpoint = "localhost",
        Port = 9000,
        AccessKey = "key",
        SecretKey = "secret",
        BucketName = "test-bucket"
    };

    [Fact]
    public void Validate_Succeeds_WhenEnvNotConfigured()
    {
        var result = _validator.Validate(Options.DefaultName, new StorageSettings());

        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [InlineData(StorageProviderIds.Minio)]
    [InlineData(StorageProviderIds.Silo)]
    [InlineData(StorageProviderIds.SeaweedFs)]
    public void Validate_Succeeds_ForSupportedProviders(string provider)
    {
        var result = _validator.Validate(Options.DefaultName, ValidSettings(provider));

        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [InlineData("S3")]
    [InlineData("azure")]
    public void Validate_Fails_WhenProviderIsUnsupported(string provider)
    {
        var settings = ValidSettings();
        settings.Provider = provider;

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("not supported");
    }

    [Fact]
    public void Validate_Fails_WhenEndpointMissing()
    {
        var settings = ValidSettings();
        settings.Endpoint = "";

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("endpoint");
    }

    [Fact]
    public void Validate_Fails_WhenPortInvalid()
    {
        var settings = ValidSettings();
        settings.Port = 0;

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Port");
    }
}
