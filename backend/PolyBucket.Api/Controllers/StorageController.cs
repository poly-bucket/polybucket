using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core.Interfaces;

namespace PolyBucket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<StorageController> _logger;

        public StorageController(IStorageService storageService, ILogger<StorageController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet("{**fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            try
            {
                _logger.LogInformation("Getting file: {FileName}", fileName);

                var fileStream = await _storageService.GetFileAsync(fileName);

                // Try to determine content type based on file extension
                var contentType = GetContentType(fileName);

                return File(fileStream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file: {FileName}", fileName);
                return NotFound(new { message = "File not found" });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded" });
            }

            try
            {
                _logger.LogInformation("Uploading file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

                using var stream = file.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}_{file.FileName}"; // Generate a unique file name
                var result = await _storageService.UploadFileAsync(fileName, stream, file.ContentType);

                return Ok(new { url = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                return StatusCode(500, new { message = "Error uploading file" });
            }
        }

        [HttpDelete("{**fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                _logger.LogInformation("Deleting file: {FileName}", fileName);

                await _storageService.DeleteFileAsync(fileName);

                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                return StatusCode(500, new { message = "Error deleting file" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" => "text/html",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".wav" => "audio/wav",
                ".obj" => "model/obj",
                ".gltf" => "model/gltf+json",
                ".glb" => "model/gltf-binary",
                ".stl" => "model/stl",
                ".fbx" => "application/octet-stream",
                ".3ds" => "application/octet-stream",
                ".blend" => "application/octet-stream",
                _ => "application/octet-stream"  // Default content type
            };
        }
    }
}