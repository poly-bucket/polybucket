using Microsoft.Extensions.Options;
using PolyBucket.Storage.Settings;
using Shouldly;

namespace PolyBucket.Storage.Tests;

public class StorageSettingsValidatorTests
{
    private readonly StorageSettingsValidator _validator = new();

    private static StorageSettings ValidMinioSettings() => new()
    {
        Provider = "MinIO",
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

    [Fact]
    public void Validate_Succeeds_ForValidMinioSettings()
    {
        var result = _validator.Validate(Options.DefaultName, ValidMinioSettings());

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenProviderIsUnsupported()
    {
        var settings = ValidMinioSettings();
        settings.Provider = "S3";

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("not supported");
    }

    [Fact]
    public void Validate_Fails_WhenEndpointMissing()
    {
        var settings = ValidMinioSettings();
        settings.Endpoint = "";

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Endpoint");
    }

    [Fact]
    public void Validate_Fails_WhenPortInvalid()
    {
        var settings = ValidMinioSettings();
        settings.Port = 0;

        var result = _validator.Validate(Options.DefaultName, settings);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Port");
    }
}
