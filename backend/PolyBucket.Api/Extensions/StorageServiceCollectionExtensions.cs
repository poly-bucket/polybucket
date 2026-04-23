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

        var provider = storageSection.GetValue<string>("Provider")?.ToLowerInvariant() ?? "minio";
        if (provider != "minio")
        {
            throw new InvalidOperationException($"Storage provider '{provider}' is not supported. Only 'minio' is currently supported.");
        }

        services.AddSingleton<IStorageService, MinioStorageService>();
        services.AddHostedService<MinioStartupConnectivityCheck>();

        return services;
    }
} 