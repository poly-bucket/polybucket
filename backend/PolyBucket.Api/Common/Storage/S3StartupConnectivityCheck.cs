using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Common.Storage;

public sealed class S3StartupConnectivityCheck(
    IOptions<StorageSettings> storageOptions,
    ILogger<S3StartupConnectivityCheck> logger) : IHostedService
{
    private readonly StorageSettings _settings = storageOptions.Value;
    private readonly ILogger<S3StartupConnectivityCheck> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = AmazonS3ClientFactory.CreateClient(storageOptions, useExternalEndpoint: false);
            await client.ListBucketsAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Object storage connectivity check succeeded for S3 endpoint {Endpoint}:{Port} and bucket {BucketName}.",
                _settings.Endpoint,
                _settings.Port,
                _settings.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Object storage connectivity check failed for S3 endpoint {Endpoint}:{Port} and bucket {BucketName}. " +
                "Verify Storage configuration (Endpoint, Port, AccessKey, SecretKey, BucketName), network reachability, and credentials.",
                _settings.Endpoint,
                _settings.Port,
                _settings.BucketName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
