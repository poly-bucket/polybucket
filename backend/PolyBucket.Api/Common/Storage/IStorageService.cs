namespace PolyBucket.Api.Common.Storage;

public interface IStorageService
{
    /// <summary>
    /// Uploads a stream to the underlying object storage and returns the object key (and/or URL).
    /// </summary>
    /// <param name="objectName">The unique key/name to store the object under. Should include any folder prefixes.</param>
    /// <param name="data">The data stream to upload. Caller is responsible for disposing.</param>
    /// <param name="contentType">MIME type of the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final accessible URL or object key, depending on implementation.</returns>
    Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads an object as a stream.
    /// </summary>
    Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from storage.
    /// </summary>
    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a time-limited pre-signed URL for accessing the object without additional credentials.
    /// </summary>
    Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default);
} 