using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage.DependencyInjection;

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
            .Bind(storageSection);

        var provider = storageSection.GetValue<string>("Provider")?.ToLowerInvariant() ?? "minio";
        if (provider != "minio" && !string.IsNullOrWhiteSpace(storageSection.GetValue<string>("Provider")))
        {
            throw new InvalidOperationException($"Storage provider '{provider}' is not supported. Only 'minio' is currently supported.");
        }

        services.AddMemoryCache();
        services.AddSingleton<IStorageSecretProtector, StorageSecretProtector>();
        services.AddSingleton<IStorageSettingsResolver, StorageSettingsResolver>();
        services.AddSingleton<IStorageService, RoutingStorageService>();
        services.AddHostedService<MinioStartupConnectivityCheck>();

        return services;
    }
}
