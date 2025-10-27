using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Services;

namespace PolyBucket.Marketplace.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Get a plugin file
        /// </summary>
        [HttpGet("{*filePath}")]
        public async Task<IActionResult> GetFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine("uploads/plugins", filePath);
                var fileBytes = await _fileService.GetPluginFileAsync(fullPath);
                
                if (fileBytes == null)
                {
                    return NotFound(new { message = "File not found" });
                }

                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
                return StatusCode(500, new { message = "Error retrieving file" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".zip" => "application/zip",
                ".json" => "application/json",
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".html" => "text/html",
                ".txt" => "text/plain",
                ".md" => "text/markdown",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
