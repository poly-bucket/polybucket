using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PolyBucket.Core.Interfaces;

namespace PolyBucket.Infrastructure.Storage
{
    public class MinioStorageOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public int PresignedUrlExpiryMinutes { get; set; } = 60;
    }
    
    public class MinioStorageService : IObjectStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioStorageOptions _options;
        
        public MinioStorageService(IOptions<MinioStorageOptions> options)
        {
            _options = options.Value;
            
            _minioClient = new MinioClient()
                .WithEndpoint(_options.Endpoint)
                .WithCredentials(_options.AccessKey, _options.SecretKey)
                .WithSSL(_options.UseSSL)
                .Build();
        }
        
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(cancellationToken);
            
            var objectName = $"{Guid.NewGuid()}-{fileName}";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);
                
            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
            
            return objectName;
        }
        
        public async Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(cancellationToken);
            
            var memoryStream = new MemoryStream();
            
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(filePath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));
                
            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
            
            memoryStream.Position = 0;
            return memoryStream;
        }
        
        public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(cancellationToken);
            
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(filePath);
                
            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
        }
        
        public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(cancellationToken);
            
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(filePath);
                    
                await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<string> GetFileUrlAsync(string filePath, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var expiryTime = expiry ?? TimeSpan.FromMinutes(_options.PresignedUrlExpiryMinutes);
            
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(filePath)
                .WithExpiry((int)expiryTime.TotalSeconds);
            
            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }
        
        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
            
            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_options.BucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            }
        }
    }
} 