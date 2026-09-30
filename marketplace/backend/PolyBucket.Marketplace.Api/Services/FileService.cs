using System.IO;

namespace PolyBucket.Marketplace.Api.Services
{
    public class FileService : IFileService
    {
        private const int MaxStoredFileNameLength = 200;
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadPath;

        public FileService(ILogger<FileService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads/plugins";
            
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> SavePluginFileAsync(IFormFile file, string pluginId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is empty or null");
                }

                var pluginDir = Path.Combine(_uploadPath, pluginId);
                Directory.CreateDirectory(pluginDir);

                var safeOriginalName = Path.GetFileName(file.FileName);
                if (string.IsNullOrEmpty(safeOriginalName))
                {
                    safeOriginalName = "upload";
                }

                safeOriginalName = TruncateFileName(safeOriginalName, MaxStoredFileNameLength);
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{safeOriginalName}";
                var filePath = Path.Combine(pluginDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Saved plugin file: {FilePath} for plugin: {PluginId}", filePath, pluginId);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving plugin file for plugin: {PluginId}", pluginId);
                throw;
            }
        }

        public async Task<bool> DeletePluginFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted plugin file: {FilePath}", filePath);
                    return true;
                }

                _logger.LogWarning("Plugin file not found: {FilePath}", filePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plugin file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<byte[]?> GetPluginFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var fileBytes = await File.ReadAllBytesAsync(filePath);
                    _logger.LogInformation("Retrieved plugin file: {FilePath}", filePath);
                    return fileBytes;
                }

                _logger.LogWarning("Plugin file not found: {FilePath}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plugin file: {FilePath}", filePath);
                return null;
            }
        }

        public Task<string> GetPluginFileUrlAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Task.FromResult(string.Empty);
            }

            var relativePath = Path.GetRelativePath(_uploadPath, filePath);
            var urlPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
            return Task.FromResult($"/api/files/{urlPath}");
        }

        private static string TruncateFileName(string fileName, int maxLength)
        {
            if (fileName.Length <= maxLength)
            {
                return fileName;
            }

            var extension = Path.GetExtension(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var maxBaseLength = maxLength - extension.Length;
            if (maxBaseLength < 1)
            {
                return extension.Length <= maxLength
                    ? extension
                    : extension[..maxLength];
            }

            return baseName[..maxBaseLength] + extension;
        }
    }
}
