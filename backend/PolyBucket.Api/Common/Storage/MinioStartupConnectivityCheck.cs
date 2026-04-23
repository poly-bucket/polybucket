using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Common.Storage;

public sealed class MinioStartupConnectivityCheck(
    IOptions<StorageSettings> storageOptions,
    ILogger<MinioStartupConnectivityCheck> logger) : IHostedService
{
    private readonly StorageSettings _settings = storageOptions.Value;
    private readonly ILogger<MinioStartupConnectivityCheck> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = new MinioClient()
                .WithEndpoint(_settings.Endpoint, _settings.Port)
                .WithCredentials(_settings.AccessKey, _settings.SecretKey)
                .WithSSL(_settings.UseSSL)
                .Build();

            var existsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
            await client.BucketExistsAsync(existsArgs, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Object storage connectivity check succeeded for MinIO endpoint {Endpoint}:{Port} and bucket {BucketName}.",
                _settings.Endpoint,
                _settings.Port,
                _settings.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Object storage connectivity check failed for MinIO endpoint {Endpoint}:{Port} and bucket {BucketName}. " +
                "Verify Storage configuration (Endpoint, Port, AccessKey, SecretKey, BucketName), network reachability, and credentials.",
                _settings.Endpoint,
                _settings.Port,
                _settings.BucketName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
