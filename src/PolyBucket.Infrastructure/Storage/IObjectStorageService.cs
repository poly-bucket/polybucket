using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Infrastructure.Storage
{
    public interface IObjectStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
        Task<string> GetFileUrlAsync(string filePath, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    }
} 