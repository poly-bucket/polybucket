namespace PolyBucket.Storage;

public sealed class StorageNotConfiguredException : InvalidOperationException
{
    public StorageNotConfiguredException()
        : base("Object storage is not configured. Complete storage setup in the admin setup wizard or system settings.")
    {
    }
}

public sealed class UnavailableStorageService : IStorageService
{
    public static UnavailableStorageService Instance { get; } = new();

    private UnavailableStorageService()
    {
    }

    public Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default) =>
        throw new StorageNotConfiguredException();

    public Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default) =>
        throw new StorageNotConfiguredException();

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken = default) =>
        throw new StorageNotConfiguredException();

    public Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default) =>
        throw new StorageNotConfiguredException();
}
