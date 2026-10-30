using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using PolyBucket.Api.Features.Files.GetSupportedExtensions.Domain;

        namespace PolyBucket.Api.Features.Files.GetSupportedExtensions.Http;

        [ApiController]
        [Route("api/files")]
        public class GetSupportedExtensionsController(IGetSupportedExtensionsService service) : ControllerBase
        {
            /// <summary>
            /// Get all supported file extensions.
            /// </summary>
            [HttpGet("extensions")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<List<string>>> GetSupportedExtensions(CancellationToken cancellationToken)
            {
                try
                {
                    return Ok(await service.GetSupportedExtensionsAsync(cancellationToken));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving supported extensions", error = ex.Message });
                }
            }
        }
