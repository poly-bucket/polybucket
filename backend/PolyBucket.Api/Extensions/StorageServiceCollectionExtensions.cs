using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Extensions;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var storageSection = configuration.GetSection("Storage");
        if (!storageSection.Exists())
        {
            storageSection = configuration.GetSection("AppSettings:Storage");
        }

        services.AddSingleton<IValidateOptions<StorageSettings>, StorageSettingsValidator>();
        services.AddOptions<StorageSettings>()
            .Bind(storageSection)
            .ValidateOnStart();

        var provider = storageSection.GetValue<string>("Provider")?.ToLowerInvariant() ?? "s3";
        if (provider is not ("s3" or "seaweedfs"))
        {
            throw new InvalidOperationException(
                $"Storage provider '{provider}' is not supported. Supported providers: S3, SeaweedFS.");
        }

        services.AddSingleton<IStorageObjectKeyResolver, StorageObjectKeyResolver>();
        services.AddSingleton<IStorageService, AwsS3StorageService>();
        services.AddHostedService<S3StartupConnectivityCheck>();

        return services;
    }
}
