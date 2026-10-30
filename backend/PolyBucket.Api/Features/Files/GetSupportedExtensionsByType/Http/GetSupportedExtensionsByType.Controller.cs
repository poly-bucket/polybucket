using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Domain;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Http;

        [ApiController]
        [Route("api/files")]
        public class GetSupportedExtensionsByTypeController(IGetSupportedExtensionsByTypeService service) : ControllerBase
        {
            /// <summary>
            /// Get supported file extensions filtered by file type/category.
            /// </summary>
            [HttpGet("extensions/by-type/{fileType}")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<List<string>>> GetSupportedExtensionsForType(string fileType, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(fileType))
                {
                    return BadRequest(new { message = "File type parameter is required" });
                }

                try
                {
                    return Ok(await service.GetSupportedExtensionsForTypeAsync(fileType, cancellationToken));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving supported extensions for type", error = ex.Message });
                }
            }
        }
