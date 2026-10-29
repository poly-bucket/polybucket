using Microsoft.Extensions.Logging;

namespace PolyBucket.Storage;

public sealed class RoutingStorageService(
    IStorageSettingsResolver settingsResolver,
    ILogger<RoutingStorageService> logger) : IStorageService
{
    private readonly IStorageSettingsResolver _settingsResolver = settingsResolver;
    private readonly ILogger<RoutingStorageService> _logger = logger;
    private readonly object _gate = new();
    private string? _cacheFingerprint;
    private IStorageService _inner = UnavailableStorageService.Instance;

    public async Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        var service = await GetInnerAsync(cancellationToken).ConfigureAwait(false);
        return await service.UploadAsync(objectName, data, contentType, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var service = await GetInnerAsync(cancellationToken).ConfigureAwait(false);
        return await service.DownloadAsync(objectName, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var service = await GetInnerAsync(cancellationToken).ConfigureAwait(false);
        await service.DeleteAsync(objectName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var service = await GetInnerAsync(cancellationToken).ConfigureAwait(false);
        return await service.GetPresignedUrlAsync(objectName, expiry, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IStorageService> GetInnerAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsResolver.GetEffectiveSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (!settings.IsConfigured)
        {
            return UnavailableStorageService.Instance;
        }

        lock (_gate)
        {
            if (_cacheFingerprint == settings.ConfigFingerprint)
            {
                return _inner;
            }

            var errors = settings.GetValidationErrors();
            if (errors.Count > 0)
            {
                _logger.LogWarning("Effective storage settings are incomplete: {Errors}", string.Join("; ", errors));
                _inner = UnavailableStorageService.Instance;
                _cacheFingerprint = settings.ConfigFingerprint;
                return _inner;
            }

            _inner = new MinioStorageService(settings.ToStorageSettings());
            _cacheFingerprint = settings.ConfigFingerprint;
            return _inner;
        }
    }
}
