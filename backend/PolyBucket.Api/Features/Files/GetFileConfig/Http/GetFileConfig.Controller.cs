using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using PolyBucket.Api.Features.Files.GetFileConfig.Domain;

        namespace PolyBucket.Api.Features.Files.GetFileConfig.Http;

        [ApiController]
        [Route("api/files")]
        public class GetFileConfigController(IGetFileConfigService service) : ControllerBase
        {
            /// <summary>
            /// Get file upload configuration including supported extensions, size limits, and storage settings.
            /// </summary>
            [HttpGet("config")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(FileConfigResponse), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<ActionResult<FileConfigResponse>> GetFileConfig(CancellationToken cancellationToken)
            {
                try
                {
                    return Ok(await service.GetFileConfigAsync(cancellationToken));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error retrieving file configuration", error = ex.Message });
                }
            }
        }
