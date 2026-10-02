using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Common.Storage;

internal static class AmazonS3ClientFactory
{
    public static IAmazonS3 CreateClient(IOptions<StorageSettings> options, bool useExternalEndpoint)
    {
        var settings = options.Value;
        var config = BuildConfig(settings, useExternalEndpoint);
        return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
    }

    internal static AmazonS3Config BuildConfig(StorageSettings settings, bool useExternalEndpoint)
    {
        var host = useExternalEndpoint
            ? (settings.ExternalEndpoint ?? settings.Endpoint)
            : settings.Endpoint;
        var port = useExternalEndpoint
            ? (settings.ExternalPort ?? settings.Port)
            : settings.Port;
        var useSsl = useExternalEndpoint
            ? (settings.ExternalUseSSL ?? settings.UseSSL)
            : settings.UseSSL;

        host = host.Trim();
        if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            host = host["http://".Length..];
        }
        else if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            useSsl = true;
            host = host["https://".Length..];
        }

        var scheme = useSsl ? "https" : "http";
        var serviceUrl = port > 0 ? $"{scheme}://{host}:{port}" : $"{scheme}://{host}";

        return new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = settings.Region,
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };
    }
}
