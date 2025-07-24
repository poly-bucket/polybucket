using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Common.Storage;

public class AwsS3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly StorageSettings _settings;

    public AwsS3StorageService(IOptions<StorageSettings> options)
    {
        _settings = options.Value;
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_settings.Region),
            ServiceURL = string.IsNullOrWhiteSpace(_settings.Endpoint) ? null : _settings.Endpoint,
            ForcePathStyle = true // support MinIO gateway or custom endpoints
        };
        _s3 = new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, config);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3, _settings.BucketName))
        {
            await _s3.PutBucketAsync(new PutBucketRequest { BucketName = _settings.BucketName }, ct);
        }
    }

    public async Task<string> UploadAsync(string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = objectName,
            InputStream = data,
            ContentType = contentType
        };
        await _s3.PutObjectAsync(request, cancellationToken);
        return objectName; // Return the object key instead of presigned URL
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var response = await _s3.GetObjectAsync(_settings.BucketName, objectName, cancellationToken);
        var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        await _s3.DeleteObjectAsync(_settings.BucketName, objectName, cancellationToken);
    }

    public Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = objectName,
            Expires = DateTime.UtcNow.Add(expiry)
        };
        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
} 