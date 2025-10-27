using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class GetSupportedExtensionsByTypeController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;

        public GetSupportedExtensionsByTypeController(PolyBucketDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get supported file extensions filtered by file type/category
        /// </summary>
        /// <param name="fileType">File type/category (e.g., "3D", "Image", "Document", "Archive")</param>
        /// <returns>List of supported file extensions for the specified type</returns>
        [HttpGet("extensions/by-type/{fileType}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<string>>> GetSupportedExtensionsForType(string fileType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileType))
                {
                    return BadRequest(new { message = "File type parameter is required" });
                }

                var extensions = await _context.FileTypeSettings
                    .Where(ft => ft.Enabled && ft.Category.ToLower() == fileType.ToLower())
                    .OrderBy(ft => ft.Priority)
                    .ThenBy(ft => ft.FileExtension)
                    .Select(ft => ft.FileExtension)
                    .ToListAsync();

                return Ok(extensions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving supported extensions for type", error = ex.Message });
            }
        }
    }
} 