using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.Configure<StorageSettings>(storageSection);
        var provider = storageSection.GetValue<string>("Provider")?.ToLowerInvariant() ?? "minio";

        switch (provider)
        {
            case "s3":
            case "aws":
                services.AddSingleton<IStorageService, AwsS3StorageService>();
                break;
            case "azureblob":
            case "azure":
            case "blob":
                services.AddSingleton<IStorageService, AzureBlobStorageService>();
                break;
            default:
                services.AddSingleton<IStorageService, MinioStorageService>();
                break;
        }

        return services;
    }
} 