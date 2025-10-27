using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class GetFileConfigController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;
        private readonly StorageSettings _storageSettings;

        public GetFileConfigController(PolyBucketDbContext context, IOptions<StorageSettings> storageSettings)
        {
            _context = context;
            _storageSettings = storageSettings.Value;
        }

        /// <summary>
        /// Get file upload configuration including supported extensions, size limits, and storage settings
        /// </summary>
        /// <returns>File configuration with supported extensions and limits</returns>
        [HttpGet("config")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileConfigResponse), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<FileConfigResponse>> GetFileConfig()
        {
            try
            {
                // Get enabled file type settings
                var fileTypes = await _context.FileTypeSettings
                    .Where(ft => ft.Enabled)
                    .OrderBy(ft => ft.Category)
                    .ThenBy(ft => ft.Priority)
                    .Select(ft => new FileTypeConfig
                    {
                        Extension = ft.FileExtension,
                        DisplayName = ft.DisplayName,
                        Description = ft.Description,
                        MimeType = ft.MimeType,
                        MaxFileSizeBytes = ft.MaxFileSizeBytes,
                        MaxPerUpload = ft.MaxPerUpload,
                        RequiresPreview = ft.RequiresPreview,
                        IsCompressible = ft.IsCompressible,
                        Category = ft.Category,
                        Priority = ft.Priority
                    })
                    .ToListAsync();

                // Group by category
                var categories = fileTypes
                    .GroupBy(ft => ft.Category)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(ft => ft.Priority).ToList()
                    );

                var response = new FileConfigResponse
                {
                    StorageProvider = _storageSettings.Provider,
                    MaxTotalFileSize = fileTypes.Any() ? fileTypes.Max(ft => ft.MaxFileSizeBytes) : 100L * 1024 * 1024, // 100MB default
                    MaxFilesPerUpload = fileTypes.Any() ? fileTypes.Max(ft => ft.MaxPerUpload) : 10,
                    SupportedExtensions = fileTypes.Select(ft => ft.Extension).ToList(),
                    FileTypes = categories,
                    GlobalSettings = new GlobalFileSettings
                    {
                        AllowMultipleFiles = true,
                        RequirePreviewFor3D = true,
                        AutoCompressLargeFiles = true,
                        MaxConcurrentUploads = 5
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving file configuration", error = ex.Message });
            }
        }
    }

    public class FileConfigResponse
    {
        public string StorageProvider { get; set; } = string.Empty;
        public long MaxTotalFileSize { get; set; }
        public int MaxFilesPerUpload { get; set; }
        public List<string> SupportedExtensions { get; set; } = new();
        public Dictionary<string, List<FileTypeConfig>> FileTypes { get; set; } = new();
        public GlobalFileSettings GlobalSettings { get; set; } = new();
    }

    public class FileTypeConfig
    {
        public string Extension { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; }
        public int MaxPerUpload { get; set; }
        public bool RequiresPreview { get; set; }
        public bool IsCompressible { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class GlobalFileSettings
    {
        public bool AllowMultipleFiles { get; set; } = true;
        public bool RequirePreviewFor3D { get; set; } = true;
        public bool AutoCompressLargeFiles { get; set; } = true;
        public int MaxConcurrentUploads { get; set; } = 5;
    }
} 