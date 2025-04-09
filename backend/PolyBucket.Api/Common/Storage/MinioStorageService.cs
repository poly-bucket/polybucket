using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using PolyBucket.Api.Settings;
using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Common.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly StorageSettings _settings;

    public MinioStorageService(IOptions<StorageSettings> options)
    {
        _settings = options.Value;

        _client = new MinioClient()
            .WithEndpoint(_settings.Endpoint, _settings.Port)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
        bool found = await _client.BucketExistsAsync(existsArgs, cancellationToken).ConfigureAwait(false);
        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
            await _client.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

        // Return a presigned URL for convenience
        return await GetPresignedUrlAsync(objectName, TimeSpan.FromHours(1), cancellationToken);
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
        // MinIO SDK uses int seconds for expiry
        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry((int)expiry.TotalSeconds);
        return await _client.PresignedGetObjectAsync(args).ConfigureAwait(false);
    }
} 