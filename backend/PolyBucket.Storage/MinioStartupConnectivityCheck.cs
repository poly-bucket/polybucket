using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Storage;

public sealed class MinioStartupConnectivityCheck(
    IStorageSettingsResolver settingsResolver,
    ILogger<MinioStartupConnectivityCheck> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (!settings.IsConfigured)
        {
            logger.LogInformation(
                "Object storage is not configured yet. Skipping connectivity check until storage settings are saved.");
            return;
        }

        var errors = settings.GetValidationErrors();
        if (errors.Count > 0)
        {
            logger.LogCritical(
                "Object storage configuration is incomplete: {Errors}. Fix storage settings and restart the API.",
                string.Join("; ", errors));
            throw new InvalidOperationException("Object storage configuration is incomplete.");
        }

        try
        {
            await MinioStorageOperations
                .TestConnectivityAsync(settings.ToStorageSettings(), cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation(
                "Object storage connectivity check succeeded for MinIO endpoint {Endpoint}:{Port} and bucket {BucketName}.",
                settings.Endpoint,
                settings.Port,
                settings.BucketName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Object storage connectivity check failed for MinIO endpoint {Endpoint}:{Port} and bucket {BucketName}.",
                settings.Endpoint,
                settings.Port,
                settings.BucketName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
