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
    public class GetSupportedExtensionsController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;

        public GetSupportedExtensionsController(PolyBucketDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all supported file extensions
        /// </summary>
        /// <returns>List of supported file extensions</returns>
        [HttpGet("extensions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<string>>> GetSupportedExtensions()
        {
            try
            {
                var extensions = await _context.FileTypeSettings
                    .Where(ft => ft.Enabled)
                    .OrderBy(ft => ft.FileExtension)
                    .Select(ft => ft.FileExtension)
                    .ToListAsync();

                return Ok(extensions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving supported extensions", error = ex.Message });
            }
        }
    }
} 