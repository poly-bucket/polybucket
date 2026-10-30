using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Storage.GetStorageSettings;
using PolyBucket.Storage.GetStorageProviders;
using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Storage.DependencyInjection;

public static class StorageFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddStorageFeatureServices(this IServiceCollection services)
    {
        services.AddTransient<IGetStorageSettingsService, GetStorageSettingsService>();
        services.AddTransient<IGetStorageProvidersService, GetStorageProvidersService>();
        services.AddTransient<IUpdateStorageSettingsRepository, UpdateStorageSettingsRepository>();
        services.AddTransient<IUpdateStorageSettingsService, UpdateStorageSettingsService>();
        services.AddTransient<ITestStorageConfigurationService, TestStorageConfigurationService>();
        return services;
    }
}
