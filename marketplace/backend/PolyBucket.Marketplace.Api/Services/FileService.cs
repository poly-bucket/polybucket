using System.IO;

namespace PolyBucket.Marketplace.Api.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadPath;

        public FileService(ILogger<FileService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads/plugins";
            
            // Ensure upload directory exists
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

                // Create plugin-specific directory
                var pluginDir = Path.Combine(_uploadPath, pluginId);
                Directory.CreateDirectory(pluginDir);

                // Generate unique filename
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}";
                var filePath = Path.Combine(pluginDir, fileName);

                // Save file
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

        public async Task<string> GetPluginFileUrlAsync(string filePath)
        {
            // Convert file path to URL
            var relativePath = Path.GetRelativePath(_uploadPath, filePath);
            var urlPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
            return $"/api/files/{urlPath}";
        }
    }
}
