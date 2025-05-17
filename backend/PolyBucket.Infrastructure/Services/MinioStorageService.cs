using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Core.Interfaces;

namespace Infrastructure.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly ILogger<MinioStorageService> _logger;
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        
        public MinioStorageService(IConfiguration configuration, ILogger<MinioStorageService> logger)
        {
            _logger = logger;
            
            var endpoint = configuration["Storage:Endpoint"] ?? "localhost";
            var port = int.Parse(configuration["Storage:Port"] ?? "9000");
            var accessKey = configuration["Storage:AccessKey"] ?? "minioadmin";
            var secretKey = configuration["Storage:SecretKey"] ?? "minioadmin";
            _bucketName = configuration["Storage:BucketName"] ?? "polybucket-uploads";
            var useSSL = bool.Parse(configuration["Storage:UseSSL"] ?? "false");
            
            _logger.LogInformation("Initializing MinIO client: {Endpoint}:{Port}, SSL: {UseSSL}", endpoint, port, useSSL);
            
            var minioEndpoint = $"{endpoint}:{port}";
            _minioClient = new MinioClient()
                .WithEndpoint(minioEndpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();
        }
        
        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            try
            {
                _logger.LogInformation("Uploading file {FileName} to MinIO bucket {BucketName}", fileName, _bucketName);
                
                // Check if bucket exists and create it if it doesn't
                bool bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs()
                        .WithBucket(_bucketName)
                );
                
                if (!bucketExists)
                {
                    _logger.LogInformation("Bucket {BucketName} does not exist, creating it", _bucketName);
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs()
                            .WithBucket(_bucketName)
                    );
                    
                    // Set the bucket policy to public
                    var policy = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{""AWS"": [""*""]}},
                                ""Action"": [""s3:GetObject""],
                                ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                            }}
                        ]
                    }}";
                    
                    await _minioClient.SetPolicyAsync(
                        new SetPolicyArgs()
                            .WithBucket(_bucketName)
                            .WithPolicy(policy)
                    );
                }
                
                // Generate a unique object name if needed
                string objectName = fileName;
                if (!objectName.StartsWith("uploads/"))
                {
                    objectName = $"uploads/{fileName}";
                }
                
                // Upload the file
                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(objectName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(fileStream.Length)
                        .WithContentType(contentType)
                );
                
                // Return the URL to access the file
                string fileUrl = $"/api/storage/{objectName}";
                _logger.LogInformation("File uploaded successfully: {FileUrl}", fileUrl);
                
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to MinIO", fileName);
                throw;
            }
        }
        
        public async Task<Stream> GetFileAsync(string fileName)
        {
            try
            {
                _logger.LogInformation("Getting file {FileName} from MinIO bucket {BucketName}", fileName, _bucketName);
                
                // If the fileName starts with /api/storage/, remove it
                if (fileName.StartsWith("/api/storage/"))
                {
                    fileName = fileName.Substring("/api/storage/".Length);
                }
                
                // Create a memory stream to store the file contents
                var memoryStream = new MemoryStream();
                
                // Get the object
                await _minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(fileName)
                        .WithCallbackStream(stream =>
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                        })
                );
                
                _logger.LogInformation("File retrieved successfully: {FileName}", fileName);
                
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileName} from MinIO", fileName);
                throw;
            }
        }
        
        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                _logger.LogInformation("Deleting file {FileName} from MinIO bucket {BucketName}", fileName, _bucketName);
                
                // If the fileName starts with /api/storage/, remove it
                if (fileName.StartsWith("/api/storage/"))
                {
                    fileName = fileName.Substring("/api/storage/".Length);
                }
                
                // Delete the object
                await _minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(fileName)
                );
                
                _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName} from MinIO", fileName);
                throw;
            }
        }
    }
} 