using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Storage;

public sealed class StorageStartupConnectivityCheck(
    IServiceScopeFactory scopeFactory,
    ILogger<StorageStartupConnectivityCheck> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var settingsResolver = scope.ServiceProvider.GetRequiredService<IStorageSettingsResolver>();
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (!settings.IsConfigured)
        {
            logger.LogInformation(
                "Object storage is not configured yet. Complete the setup wizard storage step to connect.");
            return;
        }

        var errors = settings.GetValidationErrors();
        if (errors.Count > 0)
        {
            logger.LogCritical(
                """
                Object storage configuration is incomplete and the API cannot start.
                Provider: {Provider}
                Issues: {Errors}
                Open Admin → System → Object storage or rerun the setup wizard storage step.
                """,
                settings.Provider,
                string.Join("; ", errors));
            throw new InvalidOperationException("Object storage configuration is incomplete.");
        }

        var storageSettings = settings.ToStorageSettings();
        try
        {
            await MinioStorageOperations
                .TestConnectivityAsync(storageSettings, cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation(
                """
                Object storage connectivity check succeeded.
                Provider: {Provider}
                Internal endpoint: {Endpoint}:{Port} (SSL: {UseSsl})
                Bucket: {BucketName}
                External endpoint: {ExternalEndpoint}:{ExternalPort} (SSL: {ExternalUseSsl})
                Path-style URLs: {ForcePathStyle}
                """,
                settings.Provider,
                settings.Endpoint,
                settings.Port,
                settings.UseSsl,
                settings.BucketName,
                settings.ExternalEndpoint ?? settings.Endpoint,
                settings.ExternalPort ?? settings.Port,
                settings.ExternalUseSsl ?? settings.UseSsl,
                settings.ForcePathStyle);
        }
        catch (Exception ex)
        {
            LogConnectivityFailure(settings, ex);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void LogConnectivityFailure(EffectiveStorageSettings settings, Exception ex)
    {
        var hints = settings.Provider switch
        {
            StorageProviderIds.SeaweedFs =>
                "Confirm the SeaweedFS S3 gateway is running (default port 8333), credentials match, and path-style access is enabled on the gateway if requests fail.",
            StorageProviderIds.Silo =>
                "Confirm the Silo container is running, the internal Docker hostname matches the endpoint field, and credentials match MINIO_ROOT_USER/MINIO_ROOT_PASSWORD.",
            _ =>
                "Confirm the service is reachable from the API container, the internal hostname and port are correct, and access credentials match."
        };

        logger.LogCritical(
            ex,
            """
            Object storage connectivity check failed and the API cannot start.
            Provider: {Provider}
            Internal endpoint: {Endpoint}:{Port} (SSL: {UseSsl})
            Bucket: {BucketName}
            External endpoint: {ExternalEndpoint}:{ExternalPort} (SSL: {ExternalUseSsl})
            Path-style URLs (stored preference): {ForcePathStyle}
            Hint: {Hint}
            """,
            settings.Provider,
            settings.Endpoint,
            settings.Port,
            settings.UseSsl,
            settings.BucketName,
            settings.ExternalEndpoint ?? settings.Endpoint,
            settings.ExternalPort ?? settings.Port,
            settings.ExternalUseSsl ?? settings.UseSsl,
            settings.ForcePathStyle,
            hints);
    }
}
