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

        services.AddMemoryCache();
        services.AddSingleton<IStorageSecretProtector, StorageSecretProtector>();
        services.AddScoped<IStorageSettingsResolver, StorageSettingsResolver>();
        services.AddScoped<IStorageService, RoutingStorageService>();
        services.AddHostedService<StorageStartupConnectivityCheck>();

        return services;
    }
}
