using Minio;
using Minio.DataModel.Args;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly StorageSettings _settings;

    public MinioStorageService(StorageSettings settings)
    {
        _settings = settings;
        _client = MinioStorageOperations.CreateClient(_settings);
    }

    public async Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        await MinioStorageOperations.EnsureBucketExistsAsync(_client, _settings.BucketName, cancellationToken).ConfigureAwait(false);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

        return objectName;
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(ms));

        await _client.GetObjectAsync(getObjectArgs, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName);
        await _client.RemoveObjectAsync(args, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var endpoint = _settings.ExternalEndpoint ?? _settings.Endpoint;
        var port = _settings.ExternalPort ?? _settings.Port;
        var useSsl = _settings.ExternalUseSSL ?? _settings.UseSSL;

        var externalClient = new MinioClient()
            .WithEndpoint(endpoint, port)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(useSsl)
            .Build();

        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry((int)expiry.TotalSeconds);
        return await externalClient.PresignedGetObjectAsync(args).ConfigureAwait(false);
    }
}
