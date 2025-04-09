using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Core.Configuration;
using PolyBucket.Core.Interfaces;

namespace PolyBucket.Infrastructure.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly StorageSettings _storageSettings;
        private readonly ILogger<LocalStorageService> _logger;

        public LocalStorageService(IOptions<AppSettings> appSettings, ILogger<LocalStorageService> logger)
        {
            _storageSettings = appSettings.Value.Storage;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            try
            {
                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new ArgumentException("File stream is empty or null", nameof(fileStream));
                }

                if (fileStream.Length > _storageSettings.MaxFileSize)
                {
                    throw new ArgumentException($"File exceeds maximum size of {_storageSettings.MaxFileSize / 1024 / 1024} MB", nameof(fileStream));
                }

                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!Array.Exists(_storageSettings.AllowedExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"File extension {extension} is not allowed", nameof(fileName));
                }

                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _storageSettings.BasePath);
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                _logger.LogInformation("File {FileName} uploaded successfully to {FilePath}", fileName, filePath);
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string fileName)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _storageSettings.BasePath, fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File {FileName} not found at {FilePath}", fileName, filePath);
                    return null;
                }

                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileName}", fileName);
                throw;
            }
        }

        public Task DeleteFileAsync(string fileName)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _storageSettings.BasePath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File {FileName} deleted successfully", fileName);
                }
                else
                {
                    _logger.LogWarning("File {FileName} not found at {FilePath}", fileName, filePath);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName}", fileName);
                throw;
            }
        }
    }
} 