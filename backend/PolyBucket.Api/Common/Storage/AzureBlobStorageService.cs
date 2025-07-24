using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;
using Azure.Storage;

namespace PolyBucket.Api.Common.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _container;
    private readonly StorageSettings _settings;

    public AzureBlobStorageService(IOptions<StorageSettings> options)
    {
        _settings = options.Value;
        var blobServiceClient = string.IsNullOrWhiteSpace(_settings.ConnectionString)
            ? new BlobServiceClient(new Uri(_settings.Endpoint), new Azure.Storage.StorageSharedKeyCredential(_settings.AccessKey, _settings.SecretKey))
            : new BlobServiceClient(_settings.ConnectionString);

        _container = blobServiceClient.GetBlobContainerClient(_settings.BucketName);
        _container.CreateIfNotExists();
    }

    public async Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(objectName);
        await blob.UploadAsync(data, overwrite: true, cancellationToken: cancellationToken);
        await blob.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return objectName; // Return the object key instead of presigned URL
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(objectName);
        var ms = new MemoryStream();
        await blob.DownloadToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(objectName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(objectName);
        var sas = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sas.ToString());
    }
} 